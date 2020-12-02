// den0bot (c) StanR 2020 - MIT License

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Misc")]
	public class Misc
	{
		public int GirlSeason { get; set; }
		public long GirlSeasonStart { get; set; } // IN TICKS

		[NotMapped]
		public DateTime GirlSeasonStartDate 
		{
			get
			{
				return new DateTime(GirlSeasonStart);
			}
			set
			{
				GirlSeasonStart = value.Ticks;
			}
		}
	}
}