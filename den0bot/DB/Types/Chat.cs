// den0bot (c) StanR 2020 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
	public class Chat
	{
		[PrimaryKey]
		public long Id { get; set; }

		public bool DisableAnnouncements { get; set; }

		public bool DisableEvents { get; set; }

		public string Locale { get; set; }

		public string Introduction { get; set; }
	}
}
