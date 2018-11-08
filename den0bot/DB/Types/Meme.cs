// den0bot (c) StanR 2018 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
	public class Meme
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Link { get; set; }

		public long ChatID { get; set; }

		public bool Used { get; set; }
	}
}