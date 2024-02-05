// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.DB;
using den0bot.Util;
using System.Linq;
using den0bot.DB.Types;
using Microsoft.EntityFrameworkCore;
using den0bot.Types;
using den0bot.Types.Answers;
using Microsoft.Extensions.Logging;

namespace den0bot.Modules
{
	// 2018-2021 secret santa event
	internal class ModSanta : IModule
	{
		public readonly List<string> senders = new()
		{
			"StanRiders"
		};
		public ModSanta(ILogger<IModule> logger) : base(logger)
		{
			AddCommands(new[]
			{
				new Command
				{
					Name = "santago",
					IsOwnerOnly = true,
					ActionAsync = Go
				},
				new Command
				{
					Name = "santahelp",
					ActionAsync = Help
				},
				new Command
				{
					Name = "santagift",
					ActionAsync = Gift
				}
			});
		}

		private async Task<ICommandAnswer> Go(Message msg)
		{
			var err = string.Empty;

			List<string> receivers = new List<string>(senders);

			await using var db = new Database();
			var old = await db.Santas.Select(x => x).ToArrayAsync();
			if (old.Length > 0)
			{
				db.Santas.RemoveRange(old);
				await db.SaveChangesAsync();
			}

			foreach (var sender in senders)
			{
				bool shouldAddBack = receivers.Remove(sender); // remove sender so we dont end up sending themself

				var senderID = DatabaseCache.GetUserID(sender);
				if (senderID == 0)
					return new TextCommandAnswer($"Невозможно отправить сообщение {sender}");

				var num = RNG.Next(max: receivers.Count);
				var receiver = receivers[num];

				if (!db.Santas.Any(x => x.Sender == sender))
				{
					await db.Santas.AddAsync(new Santa
					{
						Sender = sender,
						Receiver = receiver
					});
				}
				else
					err += $"Что-то пошло не так с {sender}\n";

				if (shouldAddBack)
					receivers.Add(sender);

				receivers.Remove(receiver);

				var sentMsg = await API.SendMessage($"🎄🎄🎄 Ты даришь подарок @{receiver}! 🎄🎄🎄{Environment.NewLine}{Environment.NewLine}Если не сможешь придумать что подарить, то напиши /santahelp и тебе (возможно) придет подсказка от человека которому ты даришь!", senderID);

				if (sentMsg == null)
					err += $"Сообщение не отправлено @{sender}\n";

			}

			await db.SaveChangesAsync();
			return new TextCommandAnswer(string.IsNullOrEmpty(err) ? "Отправил" : err);
		}

		private async Task<ICommandAnswer> Help(Message msg)
		{
			if (msg.Chat.Type == ChatType.Private)
			{
				await using var db = new Database();

				var receiverID = DatabaseCache.GetUserID(db.Santas.AsNoTracking().FirstOrDefault(x => x.Sender == msg.From.Username)?.Receiver);
				if (receiverID != 0 &&
				    await API.SendMessage("Твой санта не может придумать что тебе подарить. Напиши /santagift <подарок> и я передам ему твоё пожелание!", receiverID) != null)
				{
					return new TextCommandAnswer("Ждем ответа...");
				}

				return new TextCommandAnswer("Чет не получилось");
			}
			return null;
		}

		private async Task<ICommandAnswer> Gift(Message msg)
		{
			if (msg.Chat.Type == ChatType.Private)
			{
				await using var db = new Database();

				var senderID = DatabaseCache.GetUserID(db.Santas.AsNoTracking().FirstOrDefault(x => x.Receiver == msg.From.Username)?.Sender);
				if (senderID != 0)
				{
					var gift = msg.Text[11..];
					if (string.IsNullOrEmpty(gift) || string.IsNullOrWhiteSpace(gift))
						return new TextCommandAnswer("Ты пожелание-то напиши");

					if (await API.SendMessage($"Тебе передали пожелание: \"{gift}\"", senderID) != null)
						return new TextCommandAnswer("Отправил!");

				}
					
				return new TextCommandAnswer("Чет не получилось");
			}
			return null;
		}
	}
}
