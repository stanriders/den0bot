// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace den0bot.Osu
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
			public Mods Mods;

			[JsonProperty("scores")]
			public List<Score> Scores;
		}
		public enum ScoringType
		{
			ScoreV1,
			Accuracy,
			Combo,
			ScoreV2
		}
		public enum TeamMode
		{
			HeadToHead,
			Tag,
			Team,
			TeamTag
		}
	}
}
