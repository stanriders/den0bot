﻿// den0bot (c) StanR 2024 - MIT License
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapScores
	{
		[JsonProperty("scores")]
		public Score[] Scores { get; set; } = null!;

		[JsonProperty("userScore")]
		public BeatmapUserScore UserScore { get; set; } = null!;
	}
}
