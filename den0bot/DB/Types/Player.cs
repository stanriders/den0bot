// den0bot (c) StanR 2019 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
	public class Player
	{
		[PrimaryKey]
		public int TelegramID { get; set; }

		public uint OsuID { get; set; }

		//public string Topscores { get; set; }

		//public long TopscoresChatID { get; set; }
	}
}
