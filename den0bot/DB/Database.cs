// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using den0bot.DB.Types;
using den0bot.Util;
using SQLite;

namespace den0bot.DB
{
	public static class Database
	{
		private static SQLiteConnection db;
		private static string databasePath = Path.Combine(Environment.CurrentDirectory, "data.db");
		private static List<Chat> chatCache;

		public static void Init()
		{
			db = new SQLiteConnection(databasePath);
			db.CreateTable<Chat>();
			db.CreateTable<Meme>();
			db.CreateTable<Player>();
			db.CreateTable<Misc>();
			db.CreateTable<Girl>();

			// we keep whole chat table in memory because its being accessed quite often
			chatCache = db.Table<Chat>().ToList();

		}
		public static void Close() => db.Close();

		// ---
		// Chats
		// ---
		public static List<Chat> GetAllChats() => chatCache;
		public static void AddChat(long chatID)
		{
			if (chatCache.Where(x => x.Id == chatID).FirstOrDefault() == null)
			{
				var chat = new Chat
				{
					Id = chatID,
					Banlist = string.Empty,
					DisableAnnouncements = false,
					Locale = "ru"
				};
				db.Insert(chat);
				chatCache.Add(chat);

				Log.Info("Database", string.Format("Added chat '{0}' to the chat list", chatID));
			}
		}
		public static void RemoveChat(long chatID)
		{
			Chat chat = db.Table<Chat>().Where(x => x.Id == chatID).FirstOrDefault();
			if (chat != null)
			{
				db.Delete(chat);
			}
		}
		public static void ToggleAnnouncements(long chatID, bool enable)
		{
			Chat chat = db.Table<Chat>().Where(x => x.Id == chatID).FirstOrDefault();
			if (chat != null)
			{
				chat.DisableAnnouncements = !enable;
				db.Update(chat);
			}
		}
		public static string GetChatLocale(long chatID)
		{
			Chat chat = chatCache.Where(x => x.Id == chatID).FirstOrDefault();
			if (chat != null)
			{
				if (string.IsNullOrEmpty(chat.Locale))
				{
					SetChatLocale(chatID, "ru");
					return "ru";
				}
				else
				{
					return chat.Locale;
				}
			}
			return "ru";
		}

		public static void SetChatLocale(long chatID, string locale)
		{
			Chat chat = db.Table<Chat>().Where(x => x.Id == chatID).FirstOrDefault();
			if (chat != null)
			{
				chat.Locale = locale;
				db.Update(chat);

				// Update cache as well
				var cachedChat = chatCache.Where(x => x.Id == chatID).FirstOrDefault();
				if (cachedChat != null)
					cachedChat.Locale = locale;
			}
		}
		// ---
		// Memes
		// ---
		public static int GetMemeCount(long chatID) => db.Table<Meme>().Where(x => x.ChatID == chatID).Count();
		public static void AddMeme(string link, long chatID)
		{
			if (db.Table<Meme>().Where(x => x.Link == link).FirstOrDefault() == null)
			{
				db.Insert(new Meme
				{
					Link = link,
					ChatID = chatID
				});
			}
		}
		public static string GetMeme(long chatID)
		{
			List<Meme> memes = db.Table<Meme>().Where(x => x.ChatID == chatID)?.ToList();
			if (memes != null)
			{
				memes.RemoveAll(x => x.Used == true);
				if (memes.Count == 0)
				{
					ResetUsedMeme(chatID);
					return GetMeme(chatID);
				}
				else
				{
					int num = RNG.Next(memes.Count);

					SetUsedMeme(memes[num].Id);
					return memes[num].Link;
				}
			}
			return null;
		}
		public static void SetUsedMeme(int id)
		{
			Meme meme = db.Table<Meme>().Where(x => x.Id == id).First();
			if (meme != null)
			{
				meme.Used = true;
				db.Update(meme);
			}
		}
		public static void ResetUsedMeme(long chatID)
		{
			List<Meme> memes = db.Table<Meme>().Where(x => x.ChatID == chatID)?.ToList();
			foreach (Meme meme in memes)
				meme.Used = false;

			db.UpdateAll(memes);
		}

		// ---
		// Girls
		// ---
		public static int GetGirlCount(long chatID) => db.Table<Girl>().Where(x => x.ChatID == chatID).Count();
		public static void AddGirl(string link, long chatID)
		{
			if (db.Table<Girl>().Where(x => x.Link == link).FirstOrDefault() == null)
			{
				db.Insert(new Girl
				{
					Link = link,
					ChatID = chatID,
					Rating = 0
				});
			}
		}
		public static void RemoveGirl(int id)
		{
			Girl girl = db.Table<Girl>().Where(x => x.Id == id).FirstOrDefault();
			if (girl != null)
				db.Delete(girl);
		}
		public static Girl GetGirl(long chatID)
		{
			List<Girl> girls = db.Table<Girl>().Where(x => x.ChatID == chatID)?.ToList();
			if (girls != null)
			{
				girls.RemoveAll(x => x.Used == true);
				if (girls.Count == 0)
				{
					ResetUsedGirl(chatID);
					return GetGirl(chatID);
				}
				else
				{
					int num = RNG.Next(girls.Count);

					SetUsedGirl(girls[num].Id);
					return girls[num];
				}
			}
			return null;
		}
		public static Girl GetPlatinumGirl(long chatID)
		{
			List<Girl> girls = db.Table<Girl>().Where(x => x.ChatID == chatID && x.Rating >= 10)?.ToList();
			if (girls != null && girls.Count > 0)
			{
				int num = RNG.Next(girls.Count);

				SetUsedGirl(girls[num].Id);
				return girls[num];
			}
			return null;
		}
		public static void SetUsedGirl(int id)
		{
			Girl girl = db.Table<Girl>().Where(x => x.Id == id)?.First();
			if (girl != null)
			{
				girl.Used = true;
				db.Update(girl);
			}
		}
		public static void ResetUsedGirl(long chatID)
		{
			List<Girl> girls = db.Table<Girl>().Where(x => x.ChatID == chatID)?.ToList();
			foreach (Girl girl in girls)
				girl.Used = false;

			db.UpdateAll(girls);
		}
		public static void ChangeGirlRating(int id, int rating)
		{
			Girl girl = db.Table<Girl>().Where(x => x.Id == id)?.FirstOrDefault();
			if (girl != null)
			{
				girl.Rating += rating;
				/*if (girl.Rating < -10)
				{
					db.Delete(girl);
					return;
				}*/

				db.Update(girl);
			}
		}
		public static int GetGirlRating(int id)
		{
			Girl girl = db.Table<Girl>().Where(x => x.Id == id)?.FirstOrDefault();
			if (girl != null)
			{
				return girl.Rating;
			}
			return int.MinValue;
		}
		public static List<Girl> GetTopGirls(long chatID)
		{
			return db.Table<Girl>().Where(x => x.ChatID == chatID).OrderByDescending(x => x.Rating)?.ToList();
		}
		public static void RemoveRatings()
		{
			var table = db.Table<Girl>();
			foreach (var girl in table)
			{
				girl.Rating = 0;
			}
			db.UpdateAll(table);
		}
		// ---
		// Users
		// ---
		public static List<Player> GetAllPlayers(long chatID) => db.Table<Player>().Where(x => x.ChatID == chatID)?.ToList();
		public static int GetPlayerCount() => db.Table<Player>().Count();
		public static int GetPlayerCount(long chatID) => (int)db.Table<Player>().Where(x => x.ChatID == chatID)?.Count();

		private static Player GetPlayer(int ID) => db.Table<Player>().Where(x => x.Id == ID)?.FirstOrDefault();
		private static Player GetPlayer(string username) => db.Table<Player>().Where(x => x.Username.ToLower() == username.ToLower())?.FirstOrDefault();
		public static string GetPlayerFriendlyName(int ID) => GetPlayer(ID)?.FriendlyName;
		public static uint GetPlayerOsuID(int ID) => GetPlayer(ID)?.OsuID ?? 0;
		public static uint GetPlayerOsuID(string username) => GetPlayer(username)?.OsuID ?? 0;
		public static long GetPlayerChatID(int ID) => GetPlayer(ID)?.ChatID ?? 0;

		public static bool AddPlayer(string username, string name, uint osuID, long chatID)
		{
			if (db.Table<Player>().Where(x => x.FriendlyName == name).FirstOrDefault() == null)
			{
				db.Insert(new Player
				{
					FriendlyName = name,
					OsuID = osuID,
					Topscores = string.Empty,
					ChatID = chatID,
					Username = username
				});
				return true;
			}
			return false;
		}
		public static bool RemovePlayer(string name, long chatID)
		{
			Player player = db.Table<Player>().Where(x => x.FriendlyName.ToLower() == name.ToLower() && x.ChatID == chatID).FirstOrDefault();
			if (player != null)
			{
				db.Delete(player);
				return true;
			}
			return false;
		}
		public static List<Osu.Score> GetPlayerTopscores(int ID)
		{
			string storedTopscores = db.Table<Player>().Where(x => x.Id == ID)?.First().Topscores;
			if (storedTopscores != null && storedTopscores != string.Empty)
			{
				List<Osu.Score> result = new List<Osu.Score>();
				foreach (string score in storedTopscores.Split(';'))
				{
					if (score != string.Empty)
					{
						string scoreID = score.Split('-')[0];
						string date = score.Split('-')[1];
						result.Add(new Osu.Score
						{
							ScoreID = uint.Parse(scoreID),
							Date = DateTime.Parse(date, CultureInfo.GetCultureInfo("en-us"))
						});
					}
				}
				return result;
			}
			return null;
		}
		public static void SetPlayerTopscores(List<Osu.Score> scores, int ID)
		{
			Player player = db.Table<Player>().Where(x => x.Id == ID).FirstOrDefault();
			if (player != null)
			{
				string result = string.Empty;
				foreach (Osu.Score score in scores)
				{
					result += score.ScoreID + "-" + score.Date.ToString(CultureInfo.GetCultureInfo("en-us")) + ";";
				}
				player.Topscores = result;
				db.Update(player);
			}
		}

		// ---
		// Misc
		// ---
		public static int CurrentLobbyID
		{
			get
			{
				Misc table = db.Table<Misc>().FirstOrDefault();
				if (table != null)
				{
					return table.CurrentMPLobby;
				}
				else
				{
					return 0;
				}
			}
			set
			{
				Misc table = db.Table<Misc>().FirstOrDefault();
				if (table != null)
				{
					table.CurrentMPLobby = value;
					db.Update(table);
				}
				else
				{
					db.Insert(new Misc
					{
						Hi = true,
						CurrentMPLobby = value
					});
				}

			}
		}
		public static string GetCurrentTounament()
		{
			return string.Empty;
		}
	}
}
