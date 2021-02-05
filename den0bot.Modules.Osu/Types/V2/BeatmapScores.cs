using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapScores
	{
		[JsonProperty("scores")]
		public Score[] Scores { get; set; }

		[JsonProperty("userScore")]
		public BeatmapUserScore UserScore { get; set; }
	}
}
