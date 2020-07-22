// den0bot (c) StanR 2019 - MIT License
namespace den0bot.Analytics.Data.Types
{
	public class UserStatsQuery
	{
		public long Id { get; set; }
		public long Messages { get; set; }
		public long Commands { get; set; }
		public long GirlsRequested { get; set; }
		public long Stickers { get; set; }
		public long LastMessageTimestamp { get; set; }
	}

	public class UserStatsAverageQuery
	{
		public double? AverageLength { get; set; }
	}
}
