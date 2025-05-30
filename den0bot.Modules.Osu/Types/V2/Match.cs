// den0bot (c) StanR 2025 - MIT License
using System;
using den0bot.Modules.Osu.Types.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.API.Requests.Responses;

namespace den0bot.Modules.Osu.Types.V2
{
	public class Match
	{
		[JsonProperty("match")]
		public MatchInfo Info { get; set; } = null!;

		public class MatchInfo
		{
			[JsonProperty("id")]
			public long Id { get; set; }

			[JsonProperty("start_time")]
			public DateTime StartTime { get; set; }

			[JsonProperty("end_time")]
			public DateTime? EndTime { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; } = null!;
		}

		[JsonProperty("events")]
		public MatchEvent[] Events { get; set; } = null!;

		public class MatchEvent
		{
			[JsonProperty("id")]
			public ulong Id { get; set; }

			// detail

			[JsonProperty("timestamp")]
			public DateTime Timestamp { get; set; }

			[JsonProperty("user_id")]
			public long? UserId { get; set; }

			[JsonProperty("game")]
			public MatchGame? Game { get; set; }

			public class MatchGame
			{
				[JsonProperty("id")]
				public long Id { get; set; }

				[JsonProperty("start_time")]
				public DateTime StartTime { get; set; }

				[JsonProperty("end_time")]
				public DateTime? EndTime { get; set; }

				[JsonProperty("mode")]
				public Mode Mode { get; set; }

				[JsonProperty("scoring_type")]
				[JsonConverter(typeof(StringEnumConverter))]
				public ScoringType Scoring { get; set; }

				[JsonProperty("team_type")]
				[JsonConverter(typeof(StringEnumConverter))]
				public TeamMode TeamMode { get; set; }

				[JsonProperty("mods")]
				public string[] Mods { get; set; } = null!;

				[JsonProperty("beatmap")]
				public Beatmap Beatmap { get; set; } = null!;

				[JsonProperty("scores")]
				public Score[] Scores { get; set; } = null!;
			}
		}

		[JsonProperty("users")]
		public APIUser[] Users { get; set; } = null!;

		[JsonProperty("first_event_id")]
		public long FirstEventId { get; set; }

		[JsonProperty("latest_event_id")]
		public long LatestEventId { get; set; }

		[JsonProperty("current_game_id")]
		public long? CurrentGameId { get; set; }
	}
}
