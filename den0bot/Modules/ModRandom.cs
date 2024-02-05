// den0bot (c) StanR 2022 - MIT License

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
using Microsoft.Extensions.Logging;

namespace den0bot.Modules
{
	internal class ModRandom : IModule
	{
		public ModRandom(ILogger<IModule> logger) : base(logger)
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
					ActionAsync = AddMeme
				},
				new Command
				{
					Name = "meme",
					ActionAsync = (msg) => GetRandomMeme(msg.Chat)
				},
				new Command
				{
					Name = "den0saur",
					Action = _ => GetRandomDinosaur()
				}
			});
		}

		private static ICommandAnswer GetRandomDinosaur()
		{
			return RNG.Next(1, 4) switch
			{
				1 => new TextCommandAnswer("динозавр?"),
				2 => new StickerCommandAnswer("BQADAgADNAADnML7Dbv6HgazQYiIAg"),
				3 => new StickerCommandAnswer("BQADAgADMAADnML7DXy6fUB4x-sqAg"),
				_ => null,
			};
		}

		private static ICommandAnswer Roll(Message msg)
		{
			var msgArr = (msg.Text ?? msg.Caption).Split(' ');

			BigInteger roll = RNG.Next(1, 101);

			BigInteger max = 101;
			if (msgArr.Length > 1 && BigInteger.TryParse(msgArr[1], out var newMax) && newMax > 1)
			{
				max = newMax;
				roll = RNG.NextBigInteger(max + 1, 1);
			}

			if (max > 10 && roll < 100 && Localization.GetChatLocale(msg.Chat.Id) == "ru")
			{
				var (image, caption) = ImageRollAnswer((int)roll);
				if (image is not null)
				{
					return new ImageCommandAnswer
					{
						Image = image,
						Caption = caption,
						SendTextIfFailed = true
					};
				}
			}

			return new TextCommandAnswer(Localization.Get("random_roll", msg.Chat.Id) + roll);
		}

		private static async Task<ICommandAnswer> AddMeme(Message message)
		{
			long chatId = message.Chat.Id;
			string link = message.Text[7..];

			if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
			{
				await AddMemeToDatabase(link, chatId);
				return new TextCommandAnswer(Localization.Get("random_meme_added", chatId));
			}
			if (message.Type == MessageType.Photo)
			{
				await AddMemeToDatabase(message.Photo[0].FileId, chatId);
				return new TextCommandAnswer(Localization.Get("random_meme_added", chatId));
			}
			return new TextCommandAnswer(Localization.Get("random_meme_add_failed", chatId));
		}

		private static async Task<ICommandAnswer> GetRandomMeme(Telegram.Bot.Types.Chat sender)
		{
			await using var db = new Database();

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

		private static async Task AddMemeToDatabase(string link, long chatID)
		{
			await using var db = new Database();

			if (!db.Memes.Any(x => x.Link == link))
			{
				await db.Memes.AddAsync(new Meme
				{
					Link = link,
					ChatID = chatID
				});
			}
		}

		private static (string, string) ImageRollAnswer(int number)
		{
			return number switch
			{
				0 => ("https://i.imgur.com/8C3O7Fz.jpg", null),
				1 => ("https://i.imgur.com/Xn1ZKBe.jpg", null),
				2 => ("https://i.imgur.com/TlkBskN.jpg", null),
				3 => ("https://i.imgur.com/qY76HCl.png", null),
				8 => ("https://i.imgur.com/ZBZG5ND.jpg", null),
				11 => ("https://i.imgur.com/KlQzTLI.jpg", "БАРАБАННЫЕ ПАЛОЧКИ"),
				19 => ("https://i.imgur.com/aMwY20h.png", null),
				//22 => ("https://i.imgur.com/0vRNdIh.png", "ГУСИ ЛЕБЕДИ"),
				22 => ("https://i.imgur.com/o2Bo8s1.jpg", "ЛЕБЕДЬ-ГУСЕНИЦА"),
				23 => ("https://i.imgur.com/UjIybpW.jpg", null),
				25 => ("https://i.imgur.com/7T332rW.jpg", null),
				26 => ("https://i.imgur.com/eXIIEU8.jpg", null),
				27 => ("https://i.imgur.com/ynX68hO.jpg", null),
				29 => ("https://i.imgur.com/6DeCCkP.jpg", null),
				32 => ("https://i.imgur.com/Qrwypu6.jpg", null),
				33 => ("https://i.imgur.com/eH8N7gl.jpg", "КУДРИ"),
				35 => ("https://i.imgur.com/YU2h7nV.jpg", null),
				40 => ("https://i.imgur.com/LNB4P4I.jpg", null),
				43 => ("https://i.imgur.com/NqdyWlx.jpg", null),
				44 => ("https://i.imgur.com/JFGB67J.jpg", "СТУЛЬЯ"),
				52 => ("https://i.imgur.com/Gzmoc5u.jpg", null),
				53 => ("https://i.imgur.com/el0VG64.jpg", null),
				55 => ("https://i.imgur.com/dqnFluV.png", "ПЕРЧАТКИ"),
				57 => ("https://i.imgur.com/tpdY6Gq.jpg", null),
				62 => ("https://i.imgur.com/ijj9X8M.jpg", null),
				65 => ("https://i.imgur.com/A99vYJq.jpg", null),
				66 => ("https://i.imgur.com/3Cc3TL4.png", "ВАЛЕНКИ"),
				68 => ("https://i.imgur.com/7rxY0eb.jpg", null),
				69 => ("https://i.imgur.com/sDY752R.jpg", "ПЕРЕВЁРТЫШ"),
				71 => ("https://i.imgur.com/VnXpu1R.jpg", null),
				73 => ("https://i.imgur.com/lR4ddE1.jpg", null),
				77 => ("https://i.imgur.com/Vql9Qcp.jpg", "ТОПОРИКИ"),
				82 => ("https://i.imgur.com/ZuCynbz.png", null),
				85 => ("https://i.imgur.com/yFnQECR.jpg", null),
				88 => ("https://i.imgur.com/uVwTHK4.jpg", "МАТРЁШКИ"),
				99 => ("https://i.imgur.com/j4Yfkux.png", null),
				_ => (null,null),
			};
		}
	}
}
