using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapUserScore
	{
		[JsonProperty("position")]
		public int Position { get; set; }

		[JsonProperty("score")]
		public Score Score { get; set; }
	}
}
