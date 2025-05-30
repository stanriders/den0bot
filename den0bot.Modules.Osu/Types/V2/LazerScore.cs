// den0bot (c) StanR 2025 - MIT License
using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.API;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace den0bot.Modules.Osu.Types.V2
{
	public class LazerScore
	{
		[JsonProperty("id")]
		public ulong? Id { get; set; }

		[JsonProperty("user")]
		public UserShort? User { get; set; }

		[JsonProperty("user_id")]
		public uint UserId { get; set; }

		[JsonProperty("beatmap")]
		public BeatmapShort? BeatmapShort { get; set; }

		[JsonProperty("beatmapset")]
		public BeatmapSetShort? BeatmapSet { get; set; }

		[JsonProperty("rank")]
		[JsonConverter(typeof(StringEnumConverter))]
		public ScoreGrade? Grade { get; set; }

		[JsonProperty("pp")]
		public double? Pp { get; set; }

		private double accuracy = 0.0;
		[JsonProperty("accuracy")]
		public double Accuracy
		{
			get
			{
				if (accuracy <= 0.0)
				{
					/*
					 * Accuracy = Total points of hits / (Total number of hits * 300)
					 * Total points of hits  =  Number of 50s * 50 + Number of 100s * 100 + Number of 300s * 300
					 * Total number of hits  =  Number of misses + Number of 50's + Number of 100's + Number of 300's
					 */

					double totalPoints = Statistics.GetValueOrDefault(HitResult.Meh) * 50 + Statistics.GetValueOrDefault(HitResult.Ok) * 100 + Statistics.GetValueOrDefault(HitResult.Great) * 300;
					double totalHits = Statistics.GetValueOrDefault(HitResult.Miss) + Statistics.GetValueOrDefault(HitResult.Meh) + Statistics.GetValueOrDefault(HitResult.Ok) + Statistics.GetValueOrDefault(HitResult.Great);

					accuracy = totalPoints / (totalHits * 300) * 100;
				}
				
				return accuracy;
			}
			set => accuracy = value * 100.0;
		}

		[JsonProperty("max_combo")]
		public int Combo { get; set; }

		[JsonProperty("mods")]
		public APIMod[] Mods { get; set; } = Array.Empty<APIMod>();

		[JsonProperty("ended_at")]
		public DateTime? Date { get; set; }

		[JsonProperty("score")]
		public uint Points { get; set; }

		[JsonProperty("statistics")]
		public Dictionary<HitResult, int> Statistics { get; set; } = null!;

		[JsonProperty("maximum_statistics")]
		public Dictionary<HitResult, int> MaximumStatistics { get; set; } = null!;

		[JsonProperty("legacy_total_score")]
		public int? LegacyTotalScore { get; set; }

		[JsonProperty("legacy_score_id")]
		public ulong? LegacyScoreId { get; set; }

		[JsonProperty("passed")]
		public bool Passed { get; set; }

		private BeatmapShort? beatmap;
		public BeatmapShort? Beatmap 
		{ 
			get 
			{
				if (beatmap == null)
					return BeatmapShort;
				
				return beatmap;
			}
			set => beatmap = value;
		}

		public int? LeaderboardPosition { get; set; }

		public uint ComboBasedMissCount(int maxCombo, int countSliders)
		{
			// guess the number of misses + slider breaks from combo
			double comboBasedMissCount;

			if (countSliders == 0)
			{
				if (Combo < maxCombo)
					comboBasedMissCount = (double)maxCombo / Combo;
				else
					comboBasedMissCount = 0;
			}
			else
			{
				double fullComboThreshold = maxCombo - 0.1 * countSliders;
				if (Combo < fullComboThreshold)
					comboBasedMissCount = fullComboThreshold / Combo;
				else
					comboBasedMissCount = Math.Pow((maxCombo - Combo) / (0.1 * countSliders), 3);
			}

			return (uint)Math.Max(Statistics.GetValueOrDefault(HitResult.Miss), Math.Floor(comboBasedMissCount));
		}

		public ScoreInfo ToScoreInfo()
		{
			return new ScoreInfo
			{
				OnlineID = (long?)Id ?? 0,
				Accuracy = Accuracy / 100.0,
				APIMods = Mods,
				IsLegacyScore = LegacyScoreId.HasValue,
				LegacyTotalScore = LegacyTotalScore,
				MaxCombo = Combo,
				Statistics = Statistics,
				MaximumStatistics = MaximumStatistics,
			};
		}
	}
}
