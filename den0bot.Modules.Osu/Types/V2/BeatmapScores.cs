// den0bot (c) StanR 2024 - MIT License
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapScores
	{
		[JsonProperty("scores")]
		public LazerScore[] Scores { get; set; }

		[JsonProperty("userScore")]
		public BeatmapUserScore UserScore { get; set; }
	}
}
