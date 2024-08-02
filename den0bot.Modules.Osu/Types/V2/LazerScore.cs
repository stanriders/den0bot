// den0bot (c) StanR 2024 - MIT License
using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace den0bot.Modules.Osu.Types.V2
{
	public class LazerScore
	{
		[JsonProperty("id")]
		public ulong? Id { get; set; }

		[JsonProperty("user")]
		public UserShort User { get; set; } = null!;

		[JsonProperty("user_id")]
		public uint UserId { get; set; }

		[JsonProperty("beatmap")]
		public BeatmapShort BeatmapShort { get; set; } = null!;

		[JsonProperty("beatmapset")]
		public BeatmapSetShort BeatmapSet { get; set; } = null!;

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

					double totalPoints = Statistics.Count50 * 50 + Statistics.Count100 * 100 + Statistics.Count300 * 300;
					double totalHits = Statistics.CountMiss + Statistics.Count50 + Statistics.Count100 + Statistics.Count300;

					accuracy = totalPoints / (totalHits * 300) * 100;
				}
				
				return accuracy;
			}
			set => accuracy = value * 100.0;
		}

		[JsonProperty("max_combo")]
		public int Combo { get; set; }

		[JsonProperty("mods")]
		public Mod[] Mods { get; set; } = Array.Empty<Mod>();

		[JsonProperty("ended_at")]
		public DateTime? Date { get; set; }

		[JsonProperty("score")]
		public uint Points { get; set; }

		[JsonProperty("statistics")]
		public ScoreStatistics Statistics { get; set; } = null!;

		public class ScoreStatistics
		{
			[JsonProperty("meh")]
			public int Count50 { get; set; }

			[JsonProperty("ok")]
			public int Count100 { get; set; }

			[JsonProperty("great")]
			public int Count300 { get; set; }

			[JsonProperty("miss")]
			public int CountMiss { get; set; }
		}
		
		private BeatmapShort? beatmap;
		public BeatmapShort Beatmap 
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

			return (uint)Math.Max(Statistics.CountMiss, Math.Floor(comboBasedMissCount));
		}
	}
}
