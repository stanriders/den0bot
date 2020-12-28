// den0bot (c) StanR 2020 - MIT License

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using den0bot.DB;
using den0bot.DB.Types;
using den0bot.Util;
using Microsoft.EntityFrameworkCore;

namespace den0bot.Modules
{
	internal class ModRandom : IModule
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
					Name = "addmeme",
					IsAdminOnly = true,
					Action = AddMeme
				},
				new Command
				{
					Name = "meme",
					ActionAsync = (msg) => GetRandomMeme(msg.Chat)
				},
				new Command
				{
					Name = "den0saur",
					ActionAsync = (msg) => GetRandomDinosaur(msg.Chat)
				}
			});
			Log.Debug("Enabled");
		}

		private async Task<string> GetRandomDinosaur(Telegram.Bot.Types.Chat sender)
		{
			switch (RNG.Next(1, 4))
			{
				case 1: return "динозавр?";
				case 2: await API.SendSticker(new InputOnlineFile("BQADAgADNAADnML7Dbv6HgazQYiIAg"), sender.Id); break;
				case 3: await API.SendSticker(new InputOnlineFile("BQADAgADMAADnML7DXy6fUB4x-sqAg"), sender.Id); break;
			}
			return string.Empty;
		}

		private string Roll(Message msg)
		{
			string[] msgArr = msg.Text.Split(' ');

			if (msgArr.Length > 1 && BigInteger.TryParse(msgArr[1], out var max) && max > 1)
				return Localization.Get("random_roll", msg.Chat.Id) + RNG.NextBigInteger(max+1);
			
			return Localization.Get("random_roll", msg.Chat.Id) + RNG.Next(max: 101);
		}

		private string AddMeme(Message message)
		{
			long chatId = message.Chat.Id;
			string link = message.Text.Substring(7);

			if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
			{
				AddMemeToDatabase(link, chatId);
				return Localization.Get("random_meme_added", chatId);
			}
			else if (message.Type == MessageType.Photo)
			{
				AddMemeToDatabase(message.Photo[0].FileId, chatId);
				return Localization.Get("random_meme_added", chatId);
			}
			return Localization.Get("random_meme_add_failed", chatId);
		}

		private async Task<string> GetRandomMeme(Telegram.Bot.Types.Chat sender)
		{
			using (var db = new Database())
			{
				int memeCount = db.Memes.Count(x => x.ChatID == sender.Id);
				if (memeCount <= 0)
					return Localization.Get("random_no_memes", sender.Id);

				var memes = await db.Memes.ToArrayAsync();
				if (memes.All(x => x.Used))
				{
					foreach (var usedMeme in memes)
					{
						usedMeme.Used = false;
						db.Memes.Update(usedMeme);
					}
				}

				var unusedMemes = memes.Count(x => !x.Used);
				if (unusedMemes > 0)
				{
					int num = RNG.Next(max: unusedMemes);
					var meme = memes[num];

					meme.Used = true;
					db.Memes.Update(meme);

					await db.SaveChangesAsync();

					string photo = meme.Link;
					if (!string.IsNullOrEmpty(photo))
					{
						await API.SendPhoto(photo, sender.Id);
						return string.Empty;
					}

					return meme.Link;
				}

				return Localization.Get("generic_fail", sender.Id);
			}
		}

		private void AddMemeToDatabase(string link, long chatID)
		{
			using (var db = new Database())
			{
				if (!db.Memes.Any(x => x.Link == link))
				{
					db.Memes.Add(new Meme
					{
						Link = link,
						ChatID = chatID
					});
				}
			}
		}
	}
}
