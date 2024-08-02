// den0bot (c) StanR 2020 - MIT License
using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;

namespace den0bot.Analytics.Web.Models
{
	public class ChartModel
	{
		public class TimeChart
		{
			public XAxis XAxis { get; set; } = null!;
			public YAxis YAxis { get; set; } = null!;

			public List<Series> Series { get; set; } = null!;
		}

		public TimeChart Times { get; } = new();
	}
}
