// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;
using den0bot.Util;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModSettings : IModule, IReceivePhotos
	{
		public ModSettings()
		{
			AddCommands(new Command[]
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
					Action = (msg) => AddMeme(msg)
				},
				new Command()
				{
					Name = "addplayer",
					IsAdminOnly = true,
					Action = (msg) => AddPlayer(msg)
				},
				new Command()
				{
					Name = "removeplayer",
					IsAdminOnly = true,
					Action = (msg) => RemovePlayer(msg)
				},
				new Command()
				{
					Name = "updateplayer",
					IsAdminOnly = true,
					Action = (msg) => UpdatePlayer(msg)
				},
				new Command()
				{
					Name = "playerlist",
					IsAdminOnly = true,
					Action = (msg) => GetPlayerList(msg)
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
					Action = (msg) => SetLocale(msg)
				}
			});
			Log.Info(this, "Enabled");
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

		private string AddPlayer(Telegram.Bot.Types.Message message)
		{
			string[] msg = message.Text.Split(' ');
			if (msg.Length == 4)
			{
				string username = msg[1];
				string name = msg[2];
				string id = msg[3];

				if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(name))
				{
					uint osuID = 0;
					if (id != null && id != string.Empty)
					{
						try { osuID = uint.Parse(id); } catch (Exception) { }
					}

					if (username[0] == '@')
						username = username.Substring(1);

					if (Database.AddPlayer(username, name, osuID, message.Chat.Id))
						return $"{username} добавлен! Имя {name}, профиль {osuID}";
					else
						return "Че-т не вышло.";
				}
			}
			return "Ты че деб? /addplayer <юзернейм-в-тг> <имя> <осу-айди>";
		}

		private string RemovePlayer(Telegram.Bot.Types.Message message)
		{
			string name = message.Text.Substring(14);

			if (name != null && name != string.Empty)
			{
				if (Database.RemovePlayer(name, message.Chat.Id))
					return $"{name} удален.";
				else
					return $"Че-т не вышло.";
			}
			return "Ты че деб? /removeplayer <имя>";
		}

		private string UpdatePlayer(Telegram.Bot.Types.Message message)
		{
			string[] msg = message.Text.Split(' ');
			if (msg.Length == 4)
			{
				string name = msg[1];
				string username = msg[2];
				string id = msg[3];

				if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(name))
				{
					uint osuID = 0;
					if (id != null && id != string.Empty)
					{
						try { osuID = uint.Parse(id); } catch (Exception) { }
					}

					if (username[0] == '@')
						username = username.Substring(1);

					//if (Database.UpdatePlayer(username, name, osuID, message.Chat.Id))
					//	return $"{username} добавлен! Имя {name}, профиль {osuID}";
					//else
						return "Че-т не вышло.";
				}
			}
			return "Ты че деб? /updateplayer <имя> <юзернейм-в-тг> <осу-айди>";
		}
		private string GetPlayerList(Telegram.Bot.Types.Message message)
		{
			string result = string.Empty;
			List<DB.Types.Player> players = Database.GetAllPlayers(message.Chat.Id);
			foreach (DB.Types.Player player in players)
			{
				result += $"{player.FriendlyName} - /u/{player.OsuID} - {player.Topscores}{Environment.NewLine}";
			}
			return result;
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
