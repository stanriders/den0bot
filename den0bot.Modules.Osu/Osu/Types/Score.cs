// den0bot (c) StanR 2020 - MIT License
using System;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types
{
	public class Score
	{
		[JsonProperty("beatmap_id")]
		public uint BeatmapID { get; set; }
		[JsonProperty("score_id")]
		public uint ScoreID { get; set; }
		[JsonProperty("user_id")]
		public uint UserID { get; set; }

		[JsonProperty("date")]
		public DateTime Date { get; set; }

		[JsonProperty("maxcombo")]
		public uint Combo { get; set; }

		[JsonProperty("perfect")]
		public short Perfect { get; set; }

		[JsonProperty("score")]
		public uint ScorePoints { get; set; }
		[JsonProperty("count300")]
		public uint Count300 { get; set; }
		[JsonProperty("count100")]
		public uint Count100 { get; set; }
		[JsonProperty("count50")]
		public uint Count50 { get; set; }
		[JsonProperty("countmiss")]
		public uint Misses { get; set; }
		[JsonProperty("countkatu")]
		public uint CountKatu { get; set; }
		[JsonProperty("countgeki")]
		public uint CountGeki { get; set; }

		[JsonProperty("enabled_mods")]
		public Mods? EnabledMods { get; set; }
		[JsonProperty("rank")]
		public string Rank { get; set; }
		[JsonProperty("pp")]
		public double Pp { get; set; }

		// multiplayer
		[JsonProperty("slot")]
		public uint Slot { get; set; }
		[JsonProperty("team")]
		public Team Team { get; set; }
		[JsonProperty("pass")] 
		private short isPass { get; set; }

		public bool IsPass => isPass != 0;

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

		public override bool Equals(object obj)
		{
			Score b = obj as Score;
			if (ReferenceEquals(this, b))
				return true;

			if (b == null)
				return false;

			return ScoreID == b.ScoreID && Date == b.Date;
		}
		public override int GetHashCode()
		{
			return ScoreID.GetHashCode() ^ Date.GetHashCode();
		}
		public static bool operator ==(Score a, Score b)
		{
			return Equals(a, b);
		}
		public static bool operator !=(Score a, Score b)
		{
			return !Equals(a, b);
		}
	}
}
