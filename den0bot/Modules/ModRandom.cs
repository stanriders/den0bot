// den0bot (c) StanR 2019 - MIT License

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using den0bot.DB;
using den0bot.Util;
using SQLite;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModRandom : IModule
	{
		public ModRandom()
		{
			Database.CreateTable<Meme>();

			AddCommands(new[]
			{
				new Command
				{
					Name = "roll",
					Reply = true,
					Action = Roll
				},
				new Command()
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

		private async Task<string> GetRandomDinosaur(Chat sender)
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

		private string AddMeme(Telegram.Bot.Types.Message message)
		{
			long chatId = message.Chat.Id;
			string link = message.Text.Substring(7);

			if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
			{
				AddMemeToDatabase(link, chatId);
				return "Мемес добавлен!";
			}
			else if (message.Type == MessageType.Photo)
			{
				AddMemeToDatabase(message.Photo[0].FileId, chatId);
				return "Мемес добавлен!";
			}
			return "Ты че деб? /addmeme <ссылка>";
		}

		private async Task<string> GetRandomMeme(Chat sender)
		{
			int memeCount = GetMemeCountFromDatabase(sender.Id);
			if (memeCount <= 0)
				return Localization.Get("random_no_memes", sender.Id);

			string photo = GetMemeFromDatabase(sender.Id);
			if (!string.IsNullOrEmpty(photo))
			{ 
				await API.SendPhoto(photo, sender.Id);
				return string.Empty;
			}

			return Localization.Get("generic_fail", sender.Id);
		}

		#region Database
		private class Meme
		{
			[PrimaryKey, AutoIncrement]
			public int Id { get; set; }

			public string Link { get; set; }

			public long ChatID { get; set; }

			public bool Used { get; set; }
		}

		private int GetMemeCountFromDatabase(long chatID) => Database.Get<Meme>(x => x.ChatID == chatID).Count;
		private void AddMemeToDatabase(string link, long chatID)
		{
			if (Database.Exist<Meme>(x => x.Link == link))
			{
				Database.Insert(new Meme
				{
					Link = link,
					ChatID = chatID
				});
			}
		}
		private string GetMemeFromDatabase(long chatID)
		{
			List<Meme> memes = Database.Get<Meme>(x => x.ChatID == chatID);
			if (memes != null)
			{
				memes.RemoveAll(x => x.Used == true);
				if (memes.Count == 0)
				{
					ResetUsedMemeInDatabase(chatID);
					return GetMemeFromDatabase(chatID);
				}
				else
				{
					int num = RNG.Next(max: memes.Count);

					SetUsedMemeInDatabase(memes[num].Id);
					return memes[num].Link;
				}
			}
			return null;
		}
		private void SetUsedMemeInDatabase(int id)
		{
			Meme meme = Database.GetFirst<Meme>(x => x.Id == id);
			if (meme != null)
			{
				meme.Used = true;
				Database.Update(meme);
			}
		}
		private void ResetUsedMemeInDatabase(long chatID)
		{
			List<Meme> memes = Database.Get<Meme>(x => x.ChatID == chatID);
			foreach (Meme meme in memes)
				meme.Used = false;

			Database.UpdateAll(memes);
		}

		#endregion
	}
}
