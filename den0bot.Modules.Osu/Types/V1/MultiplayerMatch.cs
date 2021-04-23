// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.Enums;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V1
{
	public class MultiplayerMatch
	{
		[JsonProperty("match")]
		public MatchInfo Info { get; set; }
		[JsonProperty("games")]
		public List<Game> Games { get; set; }

		public class MatchInfo
		{
			[JsonProperty("match_id")]
			public uint ID { get; set; }
			[JsonProperty("name")]
			public string Name { get; set; }
			[JsonProperty("start_time")]
			public DateTime StartTime { get; set; }
			[JsonProperty("end_time")]
			public DateTime? EndTime { get; set; }
		}
		public class Game
		{
			[JsonProperty("game_id")]
			public uint ID { get; set; }

			[JsonProperty("start_time")]
			public DateTime StartTime { get; set; }

			[JsonProperty("end_time")]
			public DateTime? EndTime { get; set; }

			[JsonProperty("beatmap_id")]
			public uint BeatmapID { get; set; }

			[JsonProperty("play_mode")]
			public Mode Mode { get; set; }

			[JsonProperty("match_type")]
			public uint MatchType { get; set; }

			[JsonProperty("scoring_type")]
			public ScoringType Scoring { get; set; }

			[JsonProperty("team_type")]
			public TeamMode TeamMode { get; set; }

			[JsonProperty("mods")]
			public LegacyMods? Mods { get; set; }

			[JsonProperty("scores")]
			public List<Score> Scores { get; set; }
		}
		
		
	}
}
