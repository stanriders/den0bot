﻿// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.DB;
using den0bot.Modules;
using den0bot.Util;

namespace den0bot
{
	public class Bot
	{
		private readonly List<IModule> modules = new List<IModule>();
		private readonly string module_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Modules";
		public const char command_trigger = '/';

		private static bool IsOwner(string username) => (username == Config.Params.OwnerUsername);
		private static async Task<bool> IsAdmin(long chatID, string username)
		{
			if (IsOwner(username))
				return true;

			var admins = await API.GetAdmins(chatID);
			return admins.FirstOrDefault(x => x.User.Username == username && (x.CanPromoteMembers ?? true)) != null;
		}

		private static bool shouldShutdown;
		public static void Shutdown() { shouldShutdown = true; }

		public static void Main()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);
			AppDomain.CurrentDomain.UnhandledException += (s, e) => { Log.Error((e.ExceptionObject as Exception).ToString()); };
			var bot = new Bot();
			bot.Run();
		}

		public void Run()
		{
			Log.Info("________________");
			Log.Info("Starting modules...");

			List<Assembly> allAssemblies = new List<Assembly>();
			if (Directory.Exists(module_path))
				foreach (string dll in Directory.GetFiles(module_path, "*.dll"))
					allAssemblies.Add(Assembly.LoadFile(dll));

			if (Config.Params.Modules != null)
			{
				foreach (var moduleName in Config.Params.Modules)
				{
					// if it's local
					Type type = Type.GetType($"den0bot.Modules.{moduleName}", false);
					if (type == null)
					{
						// if its not local
						foreach (var ass in allAssemblies)
						{
							// we only allow subclases of IModule and only if they're in the config
							type = ass.GetTypes().FirstOrDefault(t => t.IsPublic && t.IsSubclassOf(typeof(IModule)) && t.Name == moduleName);

							if (type != null)
								break;
						}
					}
					if (type != null)
						modules.Add((IModule) Activator.CreateInstance(type));
					else
						Log.Error($"{moduleName} not found!");
				}
			}
			else
			{
				Log.Error("Module list not found!");
			}
			Log.Info("Done!");

			//Osu.IRC.Connect();

			API.OnMessage += ProcessMessage;
			API.OnCallback += ProcessCallback;

			if (API.Connect())
			{
				Log.Info("Started thinking...");
				Think();
			}
			else
			{
				Log.Error("Can't connect to Telegram API!");
			}
			Log.Info("Exiting...");
		}

		private void Think()
		{
			while (API.IsConnected && !shouldShutdown)
			{
				foreach (IModule m in modules)
				{
					m.Think();
				}
				Thread.Sleep(100);
			}
			API.Disconnect();
		}

		private bool TryEvent(long chatID, out string text)
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
			if (text != string.Empty)
				return true;

			return false;
		}

		private async void ProcessMessage(object sender, MessageEventArgs messageEventArgs)
		{
			Message msg = messageEventArgs.Message;

			if (msg == null ||
				msg.ForwardFrom != null ||
				msg.ForwardFromChat != null ||
				msg.Date < DateTime.Now.ToUniversalTime().AddSeconds(-15))
				return;

			var senderChatId = msg.Chat.Id;

			if (msg.Chat.Type != ChatType.Private)
				await DatabaseCache.AddChat(senderChatId);

			await DatabaseCache.AddUser(msg.From.Id, msg.From.Username);

			if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
			{
				string greeting = Localization.NewMemberGreeting(senderChatId, msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id);
				if (msg.NewChatMembers[0].Id == API.BotUser.Id)
					greeting = Localization.Get("generic_added_to_chat", senderChatId);

				await API.SendMessage(greeting, senderChatId, ParseMode.Html);
				return;
			}

			if (Config.Params.UseEvents && 
			    (!DatabaseCache.Chats.FirstOrDefault(x=> x.Id == senderChatId)?.DisableEvents ?? false) && 
			    msg.Text != null && 
			    msg.Text[0] == command_trigger)
			{
				// random events
				if (TryEvent(senderChatId, out var e))
				{
					await API.SendMessage(e, senderChatId);
					return;
				}
			}

			ParseMode parseMode = ParseMode.Default;
			int replyID = 0;

			string result = string.Empty;
			foreach (IModule m in modules)
			{
				if (msg.Type == MessageType.Photo)
					msg.Text = msg.Caption; // for consistency

				if (m is IReceiveAllMessages messages)
				{
					if (!(msg.Text != null && msg.Text[0] == command_trigger))
					{
						await messages.ReceiveMessage(msg);
					}
				}

				var command = m.GetCommand(msg.Text);
				if (command != null)
				{
					if ((command.IsOwnerOnly && !IsOwner(msg.From.Username)) ||
					    (command.IsAdminOnly && !await IsAdmin(msg.Chat.Id, msg.From.Username)))
					{
						// ignore admin commands from non-admins
						result = Localization.Get($"annoy_{RNG.NextNoMemory(1, 10)}", senderChatId);
						break;
					}

					// fire command's action
					if (command.Action != null)
						result = command.Action(msg);
					else if (command.ActionAsync != null)
						result = await command.ActionAsync(msg);
					else
						continue;

					// add command to statistics
					if (msg.Chat.Type != ChatType.Private)
						ModAnalytics.AddCommand(msg);

					// send result if we got any
					if (!string.IsNullOrEmpty(result))
					{
						parseMode = command.ParseMode;
						if (command.Reply)
							replyID = msg.MessageId;

						if (command.ActionResult != null)
						{
							var sentMessage = await API.SendMessage(result, senderChatId, parseMode, replyID);
							if (sentMessage != null)
							{
								// call action that receives sent message
								command.ActionResult(sentMessage);
								return;
							}
						}
						break;
					}
				}
			}

			await API.SendMessage(result, senderChatId, parseMode, replyID);
		}

		private async void ProcessCallback(object sender, CallbackQueryEventArgs callbackEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveCallback module)
				{
					var result = await module.ReceiveCallback(callbackEventArgs.CallbackQuery);
					if (!string.IsNullOrEmpty(result))
						await API.AnswerCallbackQuery(callbackEventArgs.CallbackQuery.Id, result);
				}
			}
		}
	}
}