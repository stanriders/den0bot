// den0bot (c) StanR 2019 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
	public class Girl
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Link { get; set; }

		public long ChatID { get; set; }

		public bool Used { get; set; }

		public int Rating { get; set; }

		// seasonal ratings
		public int Season { get; set; }

		public int SeasonRating { get; set; }

		public bool SeasonUsed { get; set; }
	}
}
