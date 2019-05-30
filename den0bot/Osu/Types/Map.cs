// den0bot (c) StanR 2019 - MIT License
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using den0bot.Util;

namespace den0bot.Osu.Types
{
	public class Map
	{
		[JsonProperty("beatmap_id")]
		public uint BeatmapID;
		[JsonProperty("beatmapset_id")]
		public uint BeatmapSetID;

		[JsonProperty("last_update")]
		public DateTime? UpdatedDate;
		[JsonProperty("approved_date")]
		public DateTime? RankedDate;
		[JsonProperty("approved")]
		public RankedStatus Status;

		[JsonProperty("artist")]
		public string Artist;
		[JsonProperty("title")]
		public string Title;
		[JsonProperty("version")]
		public string Difficulty;
		[JsonProperty("creator")]
		public string Creator;
		[JsonProperty("mode")]
		public Mode Mode;

		[JsonProperty("difficultyrating")]
		public double StarRating;
		[JsonProperty("diff_size")]
		private double cs { get; set; }
		[JsonProperty("diff_approach")]
		private double ar { get; set; }
		[JsonProperty("diff_overall")]
		private double od { get; set; }
		[JsonProperty("diff_drain")]
		private double hp { get; set; }

		[JsonProperty("max_combo")]
		public uint? MaxCombo;
		[JsonProperty("hit_length")]
		private uint drainLength { get; set; }
		[JsonProperty("total_length")]
		public uint TotalLength;

		[JsonProperty("bpm")]
		private double bpm { get; set; }

		public double BPM(Mods mods)
		{
			if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
			{
				return bpm * 1.5;
			}
			else if (mods.HasFlag(Mods.HT))
			{
				return bpm * 0.75;
			}
			else
			{
				return bpm;
			}
		}

		public double CS(Mods mods)
		{
			if (mods.HasFlag(Mods.HR))
			{
				return cs * 1.3;
			}
			else if (mods.HasFlag(Mods.EZ))
			{
				return cs * 0.5;
			}
			else
			{
				return cs;
			}
		}

		public double AR(Mods mods)
		{
			double finalAR = ar;

			if (mods.HasFlag(Mods.HR))
			{
				finalAR = Math.Min(finalAR * 1.4, 10);
			}
			else if (mods.HasFlag(Mods.EZ))
			{
				finalAR *= 0.5;
			}

			double ms = (11700.0 - 900 * finalAR) / 6.0;
			if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
			{
				ms /= 1.5;
				finalAR = (11700.0 - 6 * ms) / 900.0;
			}
			else if (mods.HasFlag(Mods.HT))
			{
				ms /= 0.75;
				finalAR = (11700.0 - 6 * ms) / 900.0;
			}

			return finalAR;
		}

		public double OD(Mods mods)
		{
			double finalOD = od;

			if (mods.HasFlag(Mods.HR))
			{
				finalOD = Math.Min(finalOD * 1.4, 10);
			}
			else if (mods.HasFlag(Mods.EZ))
			{
				finalOD *= 0.5;
			}

			double ms = (79.5 - 6 * finalOD);
			if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
			{
				finalOD = (79.5 - ms / 1.5) / 6;
			}
			else if (mods.HasFlag(Mods.HT))
			{
				finalOD = (79.5 - ms / 0.75) / 6;
			}
			return finalOD;
		}

		public double HP(Mods mods)
		{
			if (mods.HasFlag(Mods.HR))
			{
				return hp * 1.4;
			}
			else if (mods.HasFlag(Mods.EZ))
			{
				return hp * 0.5;
			}
			else
			{
				return hp;
			}
		}

		public TimeSpan DrainLength(Mods mods)
		{
			if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
			{
				return TimeSpan.FromSeconds((long)(drainLength * 0.6666666));
			}
			else if (mods.HasFlag(Mods.HT))
			{
				return TimeSpan.FromSeconds((long)(drainLength * 1.333333));
			}
			return TimeSpan.FromSeconds(drainLength);
		}

		public string Thumbnail => "https://assets.ppy.sh/beatmaps/" + BeatmapSetID + "/covers/card@2x.jpg";

		public string Link => "https://osu.ppy.sh/b/" + BeatmapID;

		private string fileString = null;
		public string File
		{
			get
			{
				try
				{
					if (string.IsNullOrEmpty(fileString))
						fileString = new WebClient().DownloadString("https://osu.ppy.sh/osu/" + BeatmapID);

					return fileString;
				}
				catch (Exception e)
				{
					Log.Error($"File - {e.InnerMessageIfAny()}");
					return string.Empty;
				}
			}
		}

		private byte[] fileBytes = null;
		public byte[] FileBytes
		{
			get
			{
				try
				{
					if (fileBytes == null)
					{
						var client = new WebClient {Encoding = Encoding.UTF8};
						fileBytes = client.DownloadData("https://osu.ppy.sh/osu/" + BeatmapID);
					}
					return fileBytes;
				}
				catch (Exception e)
				{
					Log.Error($"File - {e.InnerMessageIfAny()}");
					return null;
				}
			}
		}
	}
}
