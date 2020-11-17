// den0bot (c) StanR 2020 - MIT License
using System;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types.V2
{
	public class Score : IScore
	{
		[JsonProperty("id")]
		public ulong Id { get; set; }

		[JsonProperty("user")]
		public UserShort User { get; set; }

		[JsonProperty("beatmap")]
		public BeatmapShort BeatmapShort { get; set; }

		[JsonProperty("beatmapset")]
		public BeatmapSetShort BeatmapSet { get; set; }

		[JsonProperty("rank")]
		public override string Grade { get; set; }

		[JsonProperty("pp")]
		public override double? Pp { get; set; }

		private double accuracy = 0.0;
		[JsonProperty("accuracy")]
		public override double Accuracy
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

					double totalPoints = Count50 * 50 + Count100 * 100 + Count300 * 300;
					double totalHits = Misses + Count50 + Count100 + Count300;

					accuracy = totalPoints / (totalHits * 300) * 100;
				}
				
				return accuracy;
			}
			set => accuracy = value * 100.0;
		}

		[JsonProperty("max_combo")]
		public override uint Combo { get; set; }

		[JsonProperty("mods")]
		public string[] Mods { get; set; }

		[JsonProperty("created_at")]
		public override DateTime Date { get; set; }

		[JsonProperty("statistics")]
		public ScoreStatistics Statistics { get; set; }

		public class ScoreStatistics
		{
			[JsonProperty("count_50")]
			public uint Count50 { get; set; }

			[JsonProperty("count_100")]
			public uint Count100 { get; set; }

			[JsonProperty("count_300")]
			public uint Count300 { get; set; }

			[JsonProperty("count_geki")]
			public uint CountGeki { get; set; }

			[JsonProperty("count_katu")]
			public uint CountKatu { get; set; }

			[JsonProperty("count_miss")]
			public uint CountMiss { get; set; }
		}

		public override uint Count300
		{
			get => Statistics.Count300;
			set => Statistics.Count300 = value;
		}

		public override uint Count100
		{
			get => Statistics.Count100;
			set => Statistics.Count100 = value;
		}

		public override uint Count50
		{
			get => Statistics.Count50;
			set => Statistics.Count50 = value;
		}

		public override uint Misses
		{
			get => Statistics.CountMiss;
			set => Statistics.CountMiss = value;
		}

		private LegacyMods? legacyMods;
		public override LegacyMods? LegacyMods
		{
			get
			{
				if (legacyMods != null)
					return legacyMods;

				var mods = Types.LegacyMods.None;
				foreach (var mod in Mods)
				{
					mods |= (LegacyMods) Enum.Parse(typeof(LegacyMods), mod);
				}

				legacyMods = mods;
				return mods;
			}
			set { legacyMods = value; }
		}

		private IBeatmap beatmap;
		public override IBeatmap Beatmap 
		{ 
			get 
			{
				if (beatmap == null)
					return BeatmapShort;
				
				return beatmap;
			}
			set
			{
				beatmap = value;
			}
		}
	}
}
