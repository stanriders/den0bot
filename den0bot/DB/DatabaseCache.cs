// den0bot (c) StanR 2021 - MIT License
using den0bot.DB.Types;
using den0bot.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace den0bot.DB
{
	public static class DatabaseCache
	{
		private static List<Chat> chatCache;
		public static List<Chat> Chats
		{
			get
			{
				if (chatCache == null)
				{
					using (var db = new Database())
					{
						chatCache = db.Chats.AsNoTracking().ToList();
					}
				}

				return chatCache;
			}
		}

		private static List<User> userCache;
		public static List<User> Users
		{ 
			get 
			{
				if (userCache == null)
				{
					using (var db = new Database())
					{
						userCache = db.Users.AsNoTracking().ToList();
					}
				}

				return userCache;
			}
		}

		private static int? girlSeason;
		private static DateTime? girlSeasonStart;

		public static async Task AddUser(int id, string username)
		{
			if (Users.All(x => x.Username != username))
			{
				using (var db = new Database())
				{
					var user = new User
					{
						Username = username,
						TelegramID = id
					};
					Users.Add(user);
					await db.Users.AddAsync(user);
					await db.SaveChangesAsync();
				}
			}
		}
		public static int GetUserID(string username)
		{
			if (!string.IsNullOrEmpty(username))
			{
				User user = Users.Find(x => x.Username == username.Replace("@", ""));
				if (user != null)
				{
					return user.TelegramID;
				}
			}
			return 0;
		}

		public static string GetUsername(int id)
		{
			return Users.Find(x => x.TelegramID == id)?.Username;
		}

		public static int GetGirlSeason()
		{
			UpdateGirlSeason();

			return girlSeason.Value;
		}

		private static void UpdateGirlSeason()
		{
			if (girlSeason == null || girlSeasonStart?.AddMonths(1) < DateTime.Today)
			{
				girlSeasonStart = DateTime.Today;
				using (var db = new Database())
				{
					var misc = db.Misc.FirstOrDefault();
					if (misc != null)
					{
						if (misc.GirlSeasonStartDate.AddMonths(1) < DateTime.Today)
						{
							// rotate seasons
							if (misc.GirlSeason > 0)
							{
								// submit seasonal ratings into the main ones
								var table = db.Girls.Where(x => x.Season == misc.GirlSeason).ToArray();
								foreach (var girl in table)
									girl.Rating += girl.SeasonRating;

								db.Girls.UpdateRange(table);
							}

							misc.GirlSeason++;
							misc.GirlSeasonStartDate = girlSeasonStart.Value;
							db.Update(misc);
						}
						girlSeason = misc.GirlSeason;
					}
					else
					{
						db.Misc.Add(new Misc
						{
							GirlSeason = 0,
							GirlSeasonStartDate = girlSeasonStart.Value
						});
					}

					db.SaveChanges();
				}
			}
		}

		public static async Task AddChat(long chatID)
		{
			if (Chats.All(x => x.Id != chatID))
			{
				using (var db = new Database())
				{
					var chat = new Chat
					{
						Id = chatID,
						DisableAnnouncements = false,
						DisableEvents = true,
						Locale = "ru"
					};
					Chats.Add(chat);
					await db.Chats.AddAsync(chat);
					await db.SaveChangesAsync();

					Log.Information($"Added chat '{chatID}' to the chat list");
				}
			}
		}
		public static async Task RemoveChat(long chatID)
		{
			var chat = Chats.FirstOrDefault(x => x.Id == chatID);
			if (chat != null)
			{
				using (var db = new Database())
				{
					Chats.Remove(chat);
					db.Chats.Remove(chat);
					await db.SaveChangesAsync();
				}
			}
		}
	}
}
