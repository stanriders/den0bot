// den0bot (c) StanR 2019 - MIT License
using System.Text.RegularExpressions;
using den0bot.DB;
using den0bot.Osu.API.Requests;
using den0bot.Osu.Types;
using den0bot.Util;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModSettings : IModule, IReceivePhotos
	{
		private readonly Regex profileRegex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public ModSettings()
		{
			AddCommands(new[]
			{
				new Command()
				{
					Name = "disableannouncements",
					IsAdminOnly = true,
					Action = (msg) => { Database.ToggleAnnouncements(msg.Chat.Id, false); return "Понял, вырубаю"; }
				},
				new Command()
				{
					Name = "enableannouncements",
					IsAdminOnly = true,
					Action = (msg) => { Database.ToggleAnnouncements(msg.Chat.Id, true); return "Понял, врубаю"; }
				},
				new Command()
				{
					Name = "addmeme",
					IsAdminOnly = true,
					Action = AddMeme
				},
				new Command()
				{
					Name = "addme",
					Action = AddMe
				},
				new Command()
				{
					Name = "removeme",
					Action = RemoveMe
				},
				new Command()
				{
					Name = "removeplayer",
					IsOwnerOnly = true,
					Action = RemovePlayer
				},
				new Command()
				{
					Name = "shutdownnow",
					IsOwnerOnly = true,
					Action = (msg) => { Bot.Shutdown(); return string.Empty; }
				},
				new Command()
				{
					Name = "setlocale",
					IsAdminOnly = true,
					Action = SetLocale
				}
			});
			Log.Debug("Enabled");
		}

		private string AddMeme(Telegram.Bot.Types.Message message)
		{
			long chatId = message.Chat.Id;
			string link = message.Text.Substring(7);

			if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
			{
				Database.AddMeme(link, chatId);
				return "Мемес добавлен!";
			}
			else if (message.Type == MessageType.Photo)
			{
				Database.AddMeme(message.Photo[0].FileId, chatId);
				return "Мемес добавлен!";
			}
			return "Ты че деб? /addmeme <ссылка>";
		}

		private string AddMe(Telegram.Bot.Types.Message message)
		{
			Match regexMatch = profileRegex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
				string player = regexMatch.Groups[1]?.Value;
				if (!string.IsNullOrEmpty(player))
				{
					uint osuID = 0;
					if (!uint.TryParse(player, out osuID))
					{
						// if they used /u/cookiezi instead of /u/124493 we ask osu API for an ID
						Player info = Osu.WebApi.MakeAPIRequest(new GetUser
						{
							Username = player

						}).Result;

						if (info == null)
							return "Ты че деб? /addme <ссылка на профиль>";
						else
							osuID = info.ID;
					}
					if (osuID != 0)
					{
						Database.AddPlayer(message.From.Id, osuID);
						return "Добавил!";
					}
				}
			}
			return "Ты че деб? /addme <ссылка на профиль>";
		}

		private string RemoveMe(Telegram.Bot.Types.Message message)
		{
			if (Database.RemovePlayer(message.From.Id))
				return $"Удалил.";
			else
				return $"Че-т не вышло.";
		}

		private string RemovePlayer(Telegram.Bot.Types.Message message)
		{
			string name = message.Text.Substring(14);

			if (!string.IsNullOrEmpty(name))
			{
				if (Database.RemovePlayer(Database.GetUserID(name)))
					return $"{name} удален.";
				else
					return $"Че-т не вышло.";
			}
			return "Ты че деб? /removeplayer <юзернейм>";
		}

		private string GetPlayerList(Telegram.Bot.Types.Message message)
		{
			/*
			string result = string.Empty;
			List<DB.Types.Player> players = Database.GetAllPlayers(message.Chat.Id);
			foreach (DB.Types.Player player in players)
			{
				result += $"{player.FriendlyName} - /u/{player.OsuID} - {player.Topscores}{Environment.NewLine}";
			}
			return result;
			*/
			return string.Empty;
		}

		private string SetLocale(Telegram.Bot.Types.Message message)
		{
			var locale = message.Text.Substring(11);
			if (Localization.GetAvailableLocales().Contains(locale))
			{
				Database.SetChatLocale(message.Chat.Id, locale);
				return "👌";
			}
			else
			{
				return "😡";
			}
		}
	}
}
