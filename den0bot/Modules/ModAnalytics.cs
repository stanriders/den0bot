﻿// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using den0bot.Analytics.Data;
using den0bot.Analytics.Data.Types;
using den0bot.Types;
using den0bot.Types.Answers;
using den0bot.Util;

namespace den0bot.Modules
{
	internal class ModAnalytics : IModule, IReceiveAllMessages, IReceiveShutdown
	{
		private static readonly List<Message> messageBuffer = new();

		private DateTime nextFlush = DateTime.Now;
		private const int buffer_flush_interval = 3; // minutes

		public ModAnalytics()
		{
			AddCommands(new[] 
			{
				new Command
				{
					Name = "compot",
					Action = (msg) => new TextCommandAnswer($"https://stats.stanr.info/chat/{msg.Chat.Id}"),
					Reply = true
				},
				new Command
				{
					Name = "analyticsflush",
					ActionAsync = async (msg) => { await Flush(); return new TextCommandAnswer("Ok"); },
					IsOwnerOnly = true
				}
			});
		}

		public override bool Init()
		{ 
			using (var db = new AnalyticsDatabase())
				db.Database.EnsureCreated();

			return base.Init();
		}

		public Task ReceiveMessage(Telegram.Bot.Types.Message message)
		{
			if (message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
			{
				messageBuffer.Add(new Message
				{
					TelegramId = message.MessageId,
					ChatId = message.Chat.Id,
					UserId = message.From.Id,
					Timestamp = message.Date.ToUniversalTime().Ticks,
					Type = message.Type.ToDbType(),
					Command = string.Empty,
					Length = message.Text?.Length ?? 0
				});
			}
			return Task.CompletedTask;
		}

		public override void Think()
		{
			if (messageBuffer.Count > 0 && nextFlush < DateTime.Now)
			{
				_ = Flush();
				nextFlush = DateTime.Now.AddMinutes(buffer_flush_interval);
			}
		}

		private async Task Flush()
		{
			await using (var db = new AnalyticsDatabase())
			{
				await db.Messages.AddRangeAsync(messageBuffer);
				await db.SaveChangesAsync();
				messageBuffer.Clear();
			}
		}

		public static void AddCommand(Telegram.Bot.Types.Message msg)
		{
			messageBuffer.Add(new Message
			{
				TelegramId = msg.MessageId,
				ChatId = msg.Chat.Id,
				UserId = msg.From.Id,
				Timestamp = msg.Date.ToUniversalTime().Ticks,
				Type = msg.Type.ToDbType(),
				Command = msg.Text?.Split(' ')[0].Replace($"@{API.BotUser.Username}", ""),
				Length = msg.Text?.Length ?? 0
			});
		}

		public static async Task AddGirl(long chatId, long userId)
		{
			await using (var db = new AnalyticsDatabase())
			{
				await db.Girls.AddAsync(new Girl
				{
					ChatId = chatId, 
					UserId = userId
				});
				await db.SaveChangesAsync();
			}
		}

		public void Shutdown()
		{
			Flush().Wait();
		}
	}
}