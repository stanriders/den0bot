// den0bot (c) StanR 2024 - MIT License
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapUserScore
	{
		[JsonProperty("position")]
		public int Position { get; set; }

		[JsonProperty("score")]
		public LazerScore Score { get; set; }
	}
}
