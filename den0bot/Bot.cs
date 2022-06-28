// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using den0bot.DB;
using den0bot.Events;
using den0bot.Modules;
using den0bot.Util;
using den0bot.Types;
using Sentry;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using MessageEventArgs = den0bot.Events.MessageEventArgs;

namespace den0bot
{
	public class Bot
	{
		private readonly List<IModule> modules = new();
		private readonly string module_path = Path.GetDirectoryName(AppContext.BaseDirectory) + Path.DirectorySeparatorChar + "Modules";
		public const char command_trigger = '/';

		public static string[] Modules { get; private set; }

		private static bool shouldShutdown;
		private static bool shouldCrash;
		public static void Shutdown(bool crash = false) { shouldShutdown = true; shouldCrash = crash; }

		public static int Main()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);

			Log.Logger = new LoggerConfiguration()
				.WriteTo.Sentry(o => o.Dsn = Config.Params.SentryDsn)
				.WriteTo.File(@"log.txt", rollingInterval: RollingInterval.Month)
				.WriteTo.Console()
				.CreateLogger();

			AppDomain.CurrentDomain.UnhandledException += (s, e) => { Log.Error((e.ExceptionObject as Exception)?.ToString()); };
			var bot = new Bot();
			return bot.Run();
		}

		public int Run()
		{
			Log.Information("________________");
			if (!LoadModules())
				return 1;

			Log.Information("Done!");

			API.OnMessage += ProcessMessage;
			API.OnMessageEdit += ProcessMessageEdit;
			API.OnCallback += ProcessCallback;

			if (API.Connect())
			{
				Log.Information("Started thinking...");
				Think();
			}
			else
			{
				Log.Error("Can't connect to Telegram API!");
				return 1;
			}

			Log.Information("Exiting...");
			return 0;
		}

		private void Think()
		{
			while (!shouldShutdown)
			{
				foreach (IModule m in modules)
				{
					m.Think();
				}
				Thread.Sleep(100);
			}

			// shutdown
			foreach (IModule m in modules)
			{
				if (m is IReceiveShutdown mShutdown)
					mShutdown.Shutdown();
			}

			if (shouldCrash)
			{
				throw new Exception("Shutdown with crash was initiated by owner");
			}
		}

		private bool LoadModules()
		{
			Log.Information("Starting modules...");
			if (Config.Params.Modules != null)
			{
				List<Assembly> allAssemblies = new List<Assembly>();
				if (Directory.Exists(module_path))
					foreach (string dll in Directory.GetFiles(module_path, "*.dll"))
						allAssemblies.Add(Assembly.LoadFile(dll));

				foreach (var moduleName in Config.Params.Modules)
				{
					// if it's local
					Type type = Type.GetType($"den0bot.Modules.{moduleName}", false);
					if (type == null)
					{
						// if its not local
						foreach (var ass in allAssemblies)
						{
							// we only allow subclasses of IModule and only if they're in the config
							type = ass.GetTypes().FirstOrDefault(t =>
								t.IsPublic && t.IsSubclassOf(typeof(IModule)) && t.Name == moduleName);

							if (type != null)
								break;
						}
					}

					if (type != null)
					{
						var module = (IModule) Activator.CreateInstance(type);
						if (module != null && module.Init())
							modules.Add(module);
					}
					else
						Log.Error($"{moduleName} not found!");
				}

				Modules = modules.Select(x => x.GetType().Name).ToArray();

				return true;
			}

			Log.Error("Module list not found!");
			return false;
		}

		private static bool TryEvent(long chatID, out string text)
		{
			text = (RNG.NextNoMemory(0, 1000)) switch
			{
				9 => Localization.Get("event_1", chatID),
				99 => Localization.Get("event_2", chatID),
				999 => Localization.Get("event_3", chatID),
				8 => Localization.Get("event_4", chatID),
				88 => Localization.Get("event_5", chatID),
				888 => Localization.Get("event_6", chatID),
				7 => Localization.Get("event_7", chatID),
				77 => Localization.Get("event_8", chatID),
				777 => Localization.Get("event_9", chatID),
				_ => string.Empty,
			};
			return text != string.Empty;
		}

		private async void ProcessMessage(object sender, MessageEventArgs messageEventArgs)
		{
			Message msg = messageEventArgs.Message;

			if (msg == null ||
				msg.LeftChatMember != null ||
				msg.Date < DateTime.Now.ToUniversalTime().AddSeconds(-15))
				return;

			SentrySdk.ConfigureScope(scope => { scope.Contexts["Data"] = new { ProcessingMessage = msg }; });

			var senderChatId = msg.Chat.Id;
			var isForwarded = msg.ForwardFrom != null || msg.ForwardFromChat != null;

			if (msg.Chat.Type != ChatType.Private)
				await DatabaseCache.AddChat(senderChatId);

			await DatabaseCache.AddUser(msg.From.Id, msg.From.Username);

			if (!isForwarded)
			{
				if (msg.NewChatMembers is { Length: > 0 })
				{
					string greeting = Localization.NewMemberGreeting(senderChatId, msg.NewChatMembers[0].FirstName,
						msg.NewChatMembers[0].Id);
					if (msg.NewChatMembers[0].Id == API.BotUser.Id)
						greeting = Localization.Get("generic_added_to_chat", senderChatId);

					await API.SendMessage(greeting, senderChatId, ParseMode.Html);
					return;
				}

				if (Config.Params.UseEvents &&
				    (!DatabaseCache.Chats.FirstOrDefault(x => x.Id == senderChatId)?.DisableEvents ?? false) &&
				    msg.Text != null &&
				    msg.Text[0] == command_trigger &&
				    TryEvent(senderChatId, out var e))
				{
					// random events
					await API.SendMessage(e, senderChatId);
					return;
				}
			}

			var knownCommand = false;

			foreach (IModule module in modules)
			{
				if (isForwarded && module is not IReceiveForwards)
					continue;

				if (msg.Type == MessageType.Photo)
					msg.Text = msg.Caption; // for consistency

				if (module is IReceiveAllMessages messages &&
				    !(msg.Text != null && msg.Text[0] == command_trigger))
				{
					await messages.ReceiveMessage(msg);
				}

				if (await module.RunCommands(msg))
				{
					// add command to statistics
					if (msg.Chat.Type != ChatType.Private)
						ModAnalytics.AddCommand(msg);

					knownCommand = true;
					break;
				}
			}

			if (msg.Text != null && 
			    msg.Text[0] == command_trigger && 
			    !knownCommand && 
			    Localization.GetChatLocale(senderChatId) == "ru")
			{
				await API.SendSticker(new InputOnlineFile("CAACAgIAAxkBAAEFJqJiuvrBS1Ba0IU-LSAj6pLT0_qV7AACBRsAAmeSkUlGfvWEHQABiccpBA"), senderChatId);
			}
		}

		private async void ProcessCallback(object sender, CallbackQueryEventArgs callbackEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveCallbacks module)
				{
					var result = await module.ReceiveCallback(callbackEventArgs.CallbackQuery);
					if (!string.IsNullOrEmpty(result))
						await API.AnswerCallbackQuery(callbackEventArgs.CallbackQuery.Id, result);
				}
			}
		}

		private async void ProcessMessageEdit(object sender, MessageEditEventArgs messageEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveMessageEdits module)
				{
					await module.ReceiveMessageEdit(messageEventArgs.EditedMessage);
				}
			}
		}
	}
}