// den0bot (c) StanR 2021 - MIT License

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Girl")]
	public class Girl
	{
		[Key]
		public int Id { get; set; }

		public string Link { get; set; }

		public long ChatID { get; set; }

		public DateTime? LastUsed { get; set; }

		public int Rating { get; set; }

		// seasonal ratings
		public int Season { get; set; }

		public int SeasonRating { get; set; }

		public DateTime? SeasonLastUsed { get; set; }
	}
}
