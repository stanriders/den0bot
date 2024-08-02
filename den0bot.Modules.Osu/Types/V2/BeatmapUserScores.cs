// den0bot (c) StanR 2024 - MIT License
using System.Collections.Generic;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapUserScores
	{
		[JsonProperty("scores")]
		public List<LazerScore> Scores { get; set; } = null!;
	}
}
