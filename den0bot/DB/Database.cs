// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using den0bot.DB.Types;
using den0bot.Util;
using SQLite;

namespace den0bot.DB
{
	public static class Database
	{
		private static SQLiteConnection db;
		private const string database_path = "./data.db";
		private static List<Chat> chatCache;
		private static List<User> userCache;
		private static int girlSeason;

		public static void Init()
		{
			db = new SQLiteConnection(database_path);
			db.CreateTable<Chat>();
			db.CreateTable<Misc>();
			db.CreateTable<User>();

			// we keep whole chat and user tables in memory because they're being accessed quite often
			chatCache = db.Table<Chat>().ToList();
			userCache = db.Table<User>().ToList();
			
			Misc miscTable = db.Table<Misc>().FirstOrDefault();
			if (miscTable != null)
				girlSeason = miscTable.GirlSeason;
		}

		public static void CreateTable<T>()
		{
			db.CreateTable<T>();
		}

		public static void Insert(object obj)
		{
			db.Insert(obj);
		}

		public static List<T> Get<T>() where T : new()
		{
			return db.Table<T>()?.ToList();
		}

		public static List<T> Get<T>(Expression<Func<T, bool>> predicate) where T : new()
		{
			return db.Table<T>().Where(predicate)?.ToList();
		}

		public static T GetFirst<T>(Expression<Func<T, bool>> predicate) where T : new()
		{
			return db.Table<T>().FirstOrDefault(predicate);
		}

		public static bool Exist<T>(Expression<Func<T, bool>> predicate) where T : new()
		{
			return db.Table<T>().FirstOrDefault(predicate) != null;
		}

		public static void Remove(object obj)
		{
			db.Delete(obj);
		}

		public static void Remove<T>(Expression<Func<T, bool>> predicate) where T : new()
		{
			var obj = db.Table<T>().FirstOrDefault(predicate);
			if (obj != null)
				db.Delete(obj);
		}

		public static void Update(object obj)
		{
			db.Update(obj);
		}

		public static void UpdateAll(IEnumerable objs)
		{
			db.UpdateAll(objs);
		}

		public static void Close() => db.Close();

		#region Users
		public static void AddUser(int id, string username)
		{
			if (userCache.Find(x => x.Username == username) == null)
			{
				var user = new User
				{
					Username = username,
					TelegramID = id
				};
				db.Insert(user);
				userCache.Add(user);
			}
		}
		public static int GetUserID(string username)
		{
			if (!string.IsNullOrEmpty(username))
			{
				User user = userCache.Find(x => x.Username == username);
				if (user != null)
				{
					return user.TelegramID;
				}
			}
			return 0;
		}
		public static string GetUsername(int id)
		{
			User user = userCache.Find(x => x.TelegramID == id);
			if (user != null)
			{
				return user.Username;
			}
			return null;
		}

		#endregion

		#region Chats
		public static List<Chat> GetAllChats() => chatCache;
		public static void AddChat(long chatID)
		{
			if (chatCache.Find(x => x.Id == chatID) == null)
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

				Log.Info($"Added chat '{chatID}' to the chat list");
			}
		}
		public static void RemoveChat(long chatID)
		{
			Chat chat = db.Table<Chat>().FirstOrDefault(x => x.Id == chatID);
			if (chat != null)
			{
				db.Delete(chat);
			}
		}
		public static void ToggleAnnouncements(long chatID, bool enable)
		{
			Chat chat = db.Table<Chat>().FirstOrDefault(x => x.Id == chatID);
			if (chat != null)
			{
				chat.DisableAnnouncements = !enable;
				db.Update(chat);
			}
		}
		public static string GetChatLocale(long chatID)
		{
			Chat chat = chatCache.Find(x => x.Id == chatID);
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
			Chat chat = db.Table<Chat>().FirstOrDefault(x => x.Id == chatID);
			if (chat != null)
			{
				chat.Locale = locale;
				db.Update(chat);

				// Update cache as well
				var cachedChat = chatCache.Find(x => x.Id == chatID);
				if (cachedChat != null)
					cachedChat.Locale = locale;
			}
		}
		#endregion

		#region Misc
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

		public static int GirlSeason
		{
			get => girlSeason;
			set
			{
				Misc table = db.Table<Misc>().FirstOrDefault();
				if (table != null)
				{
					table.GirlSeason = value;
					table.GirlSeasonStart = DateTime.Today;
					db.Update(table);
				}

				girlSeason = value;
			}
		}
		public static DateTime GirlSeasonStartDate
		{
			get
			{
				Misc table = db.Table<Misc>().FirstOrDefault();
				if (table != null)
				{
					return table.GirlSeasonStart;
				}
				else
				{
					return default(DateTime);
				}
			}
		}
		#endregion
	}
}
