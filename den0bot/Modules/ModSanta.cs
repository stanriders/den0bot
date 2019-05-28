// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.DB;
using den0bot.Util;

namespace den0bot.Modules
{
	// 2018 secret santa event
	class ModSanta : IModule
	{
		public readonly List<string> senders = new List<string>()
		{
			"StanRiders",
			"Nufirdy",
			"orinel",
			"dusomlyser",
			"Wladek",
			"slam3085",
			"noyasine",
			"machine_ka"
		};
		public ModSanta()
		{
			AddCommands(new[]
			{
				new Command
				{
					Name = "santago",
					IsOwnerOnly = true,
					Action = Go
				},
				new Command
				{
					Name = "santahelp",
					Action = Help
				},
				new Command
				{
					Name = "santagift",
					Action = Gift
				}
			});
		}

		private string Go(Message msg)
		{
			List<string> receivers = new List<string>(senders);

			foreach (var sender in senders)
			{
				bool shouldAddBack = receivers.Remove(sender); // remove sender so we dont end up sending themself

				var senderID = Database.GetUserID(sender);
				if (senderID == 0)
					return $"Невозможно отправить сообщение {sender}";

				var num = RNG.Next(max: receivers.Count);
				var receiver = receivers[num];
				Database.AddSanta(sender, receiver);

				if (shouldAddBack)
					receivers.Add(sender);

				receivers.Remove(receiver);

				API.SendMessage($"🎄🎄🎄 Ты даришь подарок @{receiver}! 🎄🎄🎄{Environment.NewLine}{Environment.NewLine}Если не сможешь придумать что подарить, то напиши /santahelp и тебе придет подсказка", senderID).NoAwait();
			}
			return string.Empty;
		}

		private string Help(Message msg)
		{
			if (msg.Chat.Type == ChatType.Private)
			{
				var receiverID = Database.GetUserID(Database.GetSantaReceiver(msg.From.Username));
				if (receiverID != 0)
				{
					API.SendMessage($"Твой санта не может придумать что тебе подарить. Напиши /santagift <подарок> и я передам ему твоё пожелание!", receiverID).NoAwait();

					return "Ждем ответа...";
				}
			}
			return string.Empty;
		}

		private string Gift(Message msg)
		{
			if (msg.Chat.Type == ChatType.Private)
			{
				var senderID = Database.GetUserID(Database.GetSantaSender(msg.From.Username));
				if (senderID != 0)
				{
					var gift = msg.Text.Substring(11);
					if (string.IsNullOrEmpty(gift) || string.IsNullOrWhiteSpace(gift))
						return "Ты пожелание-то напиши";
					else
						API.SendMessage($"Тебе передали пожелание: \"{gift}\"", senderID).NoAwait();

					return "Отправил!";
				}
			}
			return string.Empty;
		}
	}
}
