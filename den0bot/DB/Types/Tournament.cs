// den0bot (c) StanR 2018 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
	public class Tournament
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Name { get; set; }

		public bool IsRunning { get; set; }

		public long ChatID { get; set; }
	}
}