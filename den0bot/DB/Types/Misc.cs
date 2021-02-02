// den0bot (c) StanR 2021 - MIT License

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Misc")]
	public class Misc
	{
		[Key]
		public bool Hi { get; set; } // ew
		public int GirlSeason { get; set; }
		public long GirlSeasonStart { get; set; } // IN TICKS

		[NotMapped]
		public DateTime GirlSeasonStartDate 
		{
			get => new(GirlSeasonStart);
			set => GirlSeasonStart = value.Ticks;
		}
	}
}