// den0bot (c) StanR 2020 - MIT License

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace den0bot.Modules.Osu.Types.V1
{
	public class Score : IScore
	{
		[JsonProperty("beatmap_id")]
		public uint BeatmapID { get; set; }
		[JsonProperty("score_id")]
		public uint ScoreID { get; set; }
		[JsonProperty("user_id")]
		public uint UserID { get; set; }

		[JsonProperty("date")]
		public override DateTime Date { get; set; }

		[JsonProperty("maxcombo")]
		public override uint Combo { get; set; }

		[JsonProperty("perfect")]
		public short Perfect { get; set; }

		[JsonProperty("score")]
		public uint ScorePoints { get; set; }
		[JsonProperty("count300")]
		public override uint Count300 { get; set; }
		[JsonProperty("count100")]
		public override uint Count100 { get; set; }
		[JsonProperty("count50")]
		public override uint Count50 { get; set; }
		[JsonProperty("countmiss")]
		public override uint Misses { get; set; }
		[JsonProperty("countkatu")]
		public uint CountKatu { get; set; }
		[JsonProperty("countgeki")]
		public uint CountGeki { get; set; }

		[JsonProperty("enabled_mods")]
		public override LegacyMods? LegacyMods { get; set; }

		[JsonProperty("rank")]
		[JsonConverter(typeof(StringEnumConverter))]
		public override ScoreGrade Grade { get; set; }

		[JsonProperty("pp")]
		public override double? Pp { get; set; }

		// multiplayer
		[JsonProperty("slot")]
		public uint Slot { get; set; }
		[JsonProperty("team")]
		public Team Team { get; set; }
		[JsonProperty("pass")] 
		private short isPass { get; set; }

		public override IBeatmap Beatmap { get; set; }

		public bool IsPass => isPass != 0;

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
					return accuracy;
				}
				else
				{
					return accuracy;
				}
			}
			set => accuracy = value;
		}
		private double accuracy = 0.0;
	}
}
