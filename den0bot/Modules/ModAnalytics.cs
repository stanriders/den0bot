using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using den0bot.Analytics.Data;
using den0bot.Analytics.Data.Types;
using den0bot.Util;

namespace den0bot.Modules
{
	public class ModAnalytics : IModule, IReceiveAllMessages
	{
		private static readonly List<Message> messageBuffer = new List<Message>();

		private DateTime nextFlush = DateTime.Now;
		private const int buffer_flush_interval = 1; // minute

		public ModAnalytics()
		{
			AddCommand(new Command
			{
				Name = "compot",
				Action = (msg) => $"https://stats.stanr.info/home/chat?chatId={msg.Chat.Id}",
				Reply = true
			});

			using (var db = new AnalyticsDatabase())
				db.Database.EnsureCreated();

			Log.Debug("Enabled");
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

		public static void AddMessage(Message msg)
		{
			messageBuffer.Add(msg);
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
	}
}