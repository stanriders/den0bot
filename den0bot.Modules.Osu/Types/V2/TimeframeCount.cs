// den0bot (c) StanR 2020 - MIT License
using System;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class TimeframeCount
	{
		[JsonProperty("start_date")]
		public DateTime StartDate { get; set; }

		[JsonProperty("count")]
		public uint Count { get; set; }
	}
}
