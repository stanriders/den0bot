// den0bot (c) StanR 2020 - MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Chat")]
	public class Chat
	{
		[Key]
		public long Id { get; set; }

		public bool? DisableAnnouncements { get; set; }

		public bool? DisableEvents { get; set; }

		public string Locale { get; set; }

		public string Introduction { get; set; }
	}
}
