// den0bot (c) StanR 2019 - MIT License
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using den0bot.DB;
using den0bot.Util;

namespace den0bot.Modules
{
	class ModRandom : IModule
	{
		public ModRandom()
		{
			AddCommands(new[]
			{
				new Command
				{
					Name = "roll",
					Reply = true,
					Action = Roll
				},
				new Command
				{
					Name = "meme",
					Action = (msg) => GetRandomMeme(msg.Chat)
				},
				new Command
				{
					Name = "den0saur",
					Action = (msg) => GetRandomDinosaur(msg.Chat)
				}
			});
			Log.Debug("Enabled");
		}

		private string GetRandomDinosaur(Chat sender)
		{
			switch (RNG.Next(1, 4))
			{
				case 1: return "динозавр?";
				case 2: API.SendSticker(new InputOnlineFile("BQADAgADNAADnML7Dbv6HgazQYiIAg"), sender.Id); break;
				case 3: API.SendSticker(new InputOnlineFile("BQADAgADMAADnML7DXy6fUB4x-sqAg"), sender.Id); break;
			}
			return string.Empty;
		}

		private string Roll(Message msg)
		{
			string[] msgArr = msg.Text.Split(' ');

			if (msgArr.Length > 1 && BigInteger.TryParse(msgArr[1], out var max))
				return Localization.Get("random_roll", msg.Chat.Id) + RNG.NextBigInteger(max);
			
			return Localization.Get("random_roll", msg.Chat.Id) + RNG.Next(max: 101);
		}

		private string GetRandomMeme(Chat sender)
		{
			int memeCount = Database.GetMemeCount(sender.Id);
			if (memeCount <= 0)
				return Localization.Get("random_no_memes", sender.Id);

			string photo = Database.GetMeme(sender.Id);
			if (!string.IsNullOrEmpty(photo))
			{ 
				API.SendPhoto(photo, sender);
				return string.Empty;
			}

			return Localization.Get("generic_fail", sender.Id);
		}
	}
}
