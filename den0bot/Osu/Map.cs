// den0bot (c) StanR 2018 - MIT License
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using den0bot.Util;

namespace den0bot.Osu
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

		public double BPM(string mods = default(string))
		{
			if (mods.Contains("DT") || mods.Contains("NC"))
			{
				return bpm * 1.5;
			}
			else if (mods.Contains("HT"))
			{
				return bpm * 0.75;
			}
			else
			{
				return bpm;
			}
		}

		public double CS(string mods = default(string))
		{
			if (mods.Contains("HR"))
			{
				return cs * 1.3;
			}
			else if (mods.Contains("EZ"))
			{
				return cs * 0.5;
			}
			else
			{
				return cs;
			}
		}

		public double AR(string mods = default(string))
		{
			double finalAR = ar;

			if (mods.Contains("HR"))
			{
				finalAR = Math.Min(finalAR * 1.4, 10);
			}
			else if (mods.Contains("EZ"))
			{
				finalAR *= 0.5;
			}

			double ms = (11700.0 - 900 * finalAR) / 6.0;
			if (mods.Contains("DT") || mods.Contains("NC"))
			{
				ms /= 1.5;
				finalAR = (11700.0 - 6 * ms) / 900.0;
			}
			else if (mods.Contains("HT"))
			{
				ms /= 0.75;
				finalAR = (11700.0 - 6 * ms) / 900.0;
			}

			return finalAR;
		}

		public double OD(string mods = default(string))
		{
			double finalOD = od;

			if (mods.Contains("HR"))
			{
				finalOD = Math.Min(finalOD * 1.4, 10);
			}
			else if (mods.Contains("EZ"))
			{
				finalOD *= 0.5;
			}

			double ms = (79.5 - 6 * finalOD);
			if (mods.Contains("DT") || mods.Contains("NC"))
			{
				finalOD = (79.5 - ms / 1.5) / 6;
			}
			else if (mods.Contains("HT"))
			{
				finalOD = (79.5 - ms / 0.75) / 6;
			}
			return finalOD;
		}

		public double HP(string mods = default(string))
		{
			if (mods.Contains("HR"))
			{
				return hp * 1.4;
			}
			else if (mods.Contains("EZ"))
			{
				return hp * 0.5;
			}
			else
			{
				return hp;
			}
		}

		public TimeSpan DrainLength(string mods = default(string))
		{
			if (mods.Contains("DT") || mods.Contains("NC"))
			{
				return TimeSpan.FromSeconds((long)(drainLength * 0.6666666));
			}
			else if (mods.Contains("HT"))
			{
				return TimeSpan.FromSeconds((long)(drainLength * 1.333333));
			}
			return TimeSpan.FromSeconds(drainLength);
		}

		public string Thumbnail => "https://assets.ppy.sh/beatmaps/" + BeatmapSetID + "/covers/cover.jpg";

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
					Log.Error(this, $"File - {e.InnerMessageIfAny()}");
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
					Log.Error(this, $"File - {e.InnerMessageIfAny()}");
					return null;
				}
			}
		}
	}
}
