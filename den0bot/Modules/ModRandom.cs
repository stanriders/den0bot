// den0bot (c) StanR 2021 - MIT License

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.DB;
using den0bot.DB.Types;
using den0bot.Util;
using Microsoft.EntityFrameworkCore;
using den0bot.Types;
using den0bot.Types.Answers;

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
					Action = (msg) => GetRandomDinosaur()
				}
			});
		}

		private ICommandAnswer GetRandomDinosaur()
		{
			switch (RNG.Next(1, 4))
			{
				case 1: return new TextCommandAnswer("динозавр?");
				case 2: return new StickerCommandAnswer("BQADAgADNAADnML7Dbv6HgazQYiIAg");
				case 3: return new StickerCommandAnswer("BQADAgADMAADnML7DXy6fUB4x-sqAg");
				default: return null;
			}
		}

		private ICommandAnswer Roll(Message msg)
		{
			string[] msgArr = msg.Text.Split(' ');

			if (msgArr.Length > 1 && BigInteger.TryParse(msgArr[1], out var max) && max > 1)
				return new TextCommandAnswer(Localization.Get("random_roll", msg.Chat.Id) + RNG.NextBigInteger(max+1));
			
			return new TextCommandAnswer(Localization.Get("random_roll", msg.Chat.Id) + RNG.Next(max: 101));
		}

		private ICommandAnswer AddMeme(Message message)
		{
			long chatId = message.Chat.Id;
			string link = message.Text.Substring(7);

			if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
			{
				AddMemeToDatabase(link, chatId);
				return new TextCommandAnswer(Localization.Get("random_meme_added", chatId));
			}
			else if (message.Type == MessageType.Photo)
			{
				AddMemeToDatabase(message.Photo[0].FileId, chatId);
				return new TextCommandAnswer(Localization.Get("random_meme_added", chatId));
			}
			return new TextCommandAnswer(Localization.Get("random_meme_add_failed", chatId));
		}

		private async Task<ICommandAnswer> GetRandomMeme(Telegram.Bot.Types.Chat sender)
		{
			using (var db = new Database())
			{
				int memeCount = db.Memes.Count(x => x.ChatID == sender.Id);
				if (memeCount <= 0)
					return Localization.GetAnswer("random_no_memes", sender.Id);

				var memes = await db.Memes.Where(x => x.ChatID == sender.Id).ToArrayAsync();
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
						return new ImageCommandAnswer()
						{
							Image = photo
						};
					}
				}

				return Localization.GetAnswer("generic_fail", sender.Id);
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
