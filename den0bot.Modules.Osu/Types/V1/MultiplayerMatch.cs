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
		public MatchInfo Info;
		[JsonProperty("games")]
		public List<Game> Games;

		public class MatchInfo
		{
			[JsonProperty("match_id")]
			public uint ID;
			[JsonProperty("name")]
			public string Name;
			[JsonProperty("start_time")]
			public DateTime StartTime;
			[JsonProperty("end_time")]
			public DateTime? EndTime;
		}
		public class Game
		{
			[JsonProperty("game_id")]
			public uint ID;

			[JsonProperty("start_time")]
			public DateTime StartTime;

			[JsonProperty("end_time")]
			public DateTime? EndTime;

			[JsonProperty("beatmap_id")]
			public uint BeatmapID;

			[JsonProperty("play_mode")]
			public Mode Mode;

			[JsonProperty("match_type")]
			public uint MatchType;

			[JsonProperty("scoring_type")]
			public ScoringType Scoring;

			[JsonProperty("team_type")]
			public TeamMode TeamMode;

			[JsonProperty("mods")]
			public LegacyMods? Mods;

			[JsonProperty("scores")]
			public List<Score> Scores;
		}
		
		
	}
}
