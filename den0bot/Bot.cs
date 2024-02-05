// den0bot (c) StanR 2024 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Events;
using den0bot.Modules;
using den0bot.Util;
using den0bot.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;
using MessageEventArgs = den0bot.Events.MessageEventArgs;
using Serilog.Context;
using Message = Telegram.Bot.Types.Message;

namespace den0bot
{
	public class Bot : BackgroundService
	{
		public const char command_trigger = '/';

		private static bool shouldShutdown;
		private static bool shouldCrash;

		private readonly List<IModule> modules;
		private readonly ILogger<Bot> logger;
		private readonly IHostApplicationLifetime lifetime;
		private readonly ConfigFile config;

		public static void Shutdown(bool crash = false) { shouldShutdown = true; shouldCrash = crash; }

		public Bot(ILogger<Bot> logger, IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, IOptions<ConfigFile> options)
		{
			this.logger = logger;
			this.lifetime = lifetime;
			config = options.Value;

			modules = serviceProvider.GetServices<IModule>().ToList();
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			if (!API.Connect(config?.TelegamToken))
			{
				throw new Exception("Couldn't connect to telegram API!");
			}

			LoadModules();

			API.OnMessage += ProcessMessage;
			API.OnMessageEdit += ProcessMessageEdit;
			API.OnCallback += ProcessCallback;

			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
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

			logger.LogInformation("Exiting...");

			return base.StopAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!shouldShutdown && !stoppingToken.IsCancellationRequested)
			{
				foreach (IModule m in modules)
				{
					m.Think();
				}
				await Task.Delay(100, stoppingToken);
			}

			lifetime.StopApplication();
		}
		
		private void LoadModules()
		{
			logger.LogInformation("Starting modules...");
			foreach (var module in modules)
			{
				if (!module.Init())
				{
					modules.Remove(module);
				}
			}
			
			logger.LogInformation("Done!");
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

			using var _ = LogContext.PushProperty("Data", new
			{
				ProcessingMessage = JsonConvert.SerializeObject(msg)
			});
			
			var text = msg.Text ?? msg.Caption;

			var senderChatId = msg.Chat.Id;
			var isForwarded = msg.ForwardFrom != null || msg.ForwardFromChat != null;

			if (msg.Chat.Type != ChatType.Private)
				await DatabaseCache.AddChat(senderChatId);

			await DatabaseCache.AddUser(msg.From.Id, msg.From.Username);

			if (!isForwarded)
			{
				if (msg.NewChatMembers is { Length: > 0 })
				{
					string greeting = Localization.NewMemberGreeting(senderChatId, msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id);
					if (msg.NewChatMembers[0].Id == API.BotUser.Id)
						greeting = Localization.Get("generic_added_to_chat", senderChatId);

					await API.SendMessage(greeting, senderChatId, ParseMode.Html);
					return;
				}

				if (config.UseEvents &&
				    (!DatabaseCache.Chats.FirstOrDefault(x => x.Id == senderChatId)?.DisableEvents ?? false) &&
				    text != null &&
				    text[0] == command_trigger &&
				    TryEvent(senderChatId, out var e))
				{
					// random events
					await API.SendMessage(e, senderChatId);
					return;
				}
			}

			foreach (IModule module in modules)
			{
				// ReSharper disable once SuspiciousTypeConversion.Global
				if (isForwarded && module is not IReceiveForwards)
					continue;

				if (module is IReceiveAllMessages messages &&
				    !(text != null && text[0] == command_trigger))
				{
					await messages.ReceiveMessage(msg);
				}

				if (await module.RunCommands(msg))
				{
					// add command to statistics
					if (msg.Chat.Type != ChatType.Private)
						ModAnalytics.AddCommand(msg);

					break;
				}
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
				// ReSharper disable once SuspiciousTypeConversion.Global
				if (m is IReceiveMessageEdits module)
				{
					await module.ReceiveMessageEdit(messageEventArgs.EditedMessage);
				}
			}
		}
	}
}