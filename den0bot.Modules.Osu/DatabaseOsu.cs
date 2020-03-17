// den0bot (c) StanR 2020 - MIT License
using den0bot.DB;
using SQLite;

namespace den0bot.Modules.Osu
{
	public static class DatabaseOsu
	{
		public class Player
		{
			[PrimaryKey]
			public int TelegramID { get; set; }
			public uint OsuID { get; set; }
		}

		static DatabaseOsu()
		{
			Database.CreateTable<Player>();
		}

		public static Player GetPlayerFromDatabase(int ID) => Database.GetFirst<Player>(x => x.TelegramID == ID);

		public static uint GetPlayerOsuIDFromDatabase(int ID) => GetPlayerFromDatabase(ID)?.OsuID ?? 0;

		public static bool AddPlayerToDatabase(int tgID, uint osuID)
		{
			if (!Database.Exist<Player>(x => x.TelegramID == tgID))
			{
				Database.Insert(new Player
				{
					TelegramID = tgID,
					OsuID = osuID,
				});
				return true;
			}

			return false;
		}

		public static bool RemovePlayerFromDatabase(int tgID)
		{
			Database.Remove<Player>(x => x.TelegramID == tgID);
			if (!Database.Exist<Player>(x => x.TelegramID == tgID))
				return true;

			return false;
		}
	}
}
