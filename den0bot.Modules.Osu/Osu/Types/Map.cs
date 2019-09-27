// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using den0bot.Util;

namespace den0bot.Modules.Osu.Osu.Types
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

		private static readonly Regex linkRegex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/([b,s]|(?>beatmaps)|(?>beatmapsets))\/(\d+\/?\#osu\/)?(\d+)?\/?(?>[&,?].=\d)?\s?(?>\+(\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static uint GetIdFromLink(string link, out bool isSet, out Mods mods)
		{
			isSet = false;
			mods = Mods.None;
			Match regexMatch = linkRegex.Match(link);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.Values.Where(x => (x != null) && (x.Length > 0))
					.ToList();

				bool isNew = regexGroups[1].Value == "beatmapsets"; // are we using new website or not
				uint beatmapId = 0;

				if (isNew)
				{
					if (regexGroups[2].Value.Contains("#osu/"))
					{
						beatmapId = uint.Parse(regexGroups[3].Value);
						if (regexGroups.Count > 4)
							mods = ConvertToMods(regexGroups[4].Value);
					}
					else
					{
						isSet = true;
						beatmapId = uint.Parse(regexGroups[2].Value);
						if (regexGroups.Count > 3)
							mods = ConvertToMods(regexGroups[3].Value);
					}
				}
				else
				{
					if (regexGroups[1].Value == "s")
						isSet = true;

					beatmapId = uint.Parse(regexGroups[2].Value);
					if (regexGroups.Count > 3)
						mods = ConvertToMods(regexGroups[3].Value);
				}

				return beatmapId;
			}

			return 0;
		}

		private static Mods ConvertToMods(string mods)
		{
			if (Enum.TryParse(mods, true, out Mods result) || string.IsNullOrEmpty(mods) || mods.Length > 36) // every mod combination possible
				return result;
			else
			{
				StringBuilder builder = new StringBuilder(mods.Length * 2);
				bool secondChar = false;
				foreach (char c in mods)
				{
					builder.Append(c);
					if (secondChar)
					{
						builder.Append(',');
						builder.Append(' ');
					}
					secondChar = !secondChar;
				}
				builder.Remove(builder.Length - 2, 2);
				Enum.TryParse(builder.ToString(), true, out result);
				return result;
			}
		}

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
						fileString = Web.DownloadString("https://osu.ppy.sh/osu/" + BeatmapID).Result;

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
						fileBytes = Web.DownloadBytes("https://osu.ppy.sh/osu/" + BeatmapID).Result;
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

		public string GetFormattedMapInfo(Mods mods)
		{
			string pp = string.Empty;

			try
			{
				double info100 = Oppai.GetBeatmapPP(this, mods, 100);
				if (info100 > 0)
				{
					pp = $"100% - {info100:N2}pp";

					double info98 = Oppai.GetBeatmapPP(this, mods, 98);
					if (info98 > 0)
						pp += $" | 98% - {info98:N2}pp";

					double info95 = Oppai.GetBeatmapPP(this, mods, 95);
					if (info95 > 0)
						pp += $" | 95% - {info95:N2}pp";
				}
			}
			catch (Exception e)
			{
				Log.Error($"Oppai failed: {e.InnerMessageIfAny()}");
			}

			return
				$"[{Difficulty.FilterToHTML()}] - {StarRating:N2}* - {DrainLength(mods):mm\':\'ss} - {Creator} - <b>{Status.ToString()}</b>\n" +
				$"<b>CS:</b> {CS(mods):N2} | <b>AR:</b> {AR(mods):N2} | <b>OD:</b> {OD(mods):N2} | <b>BPM:</b> {BPM(mods):N2}\n" +
				$"{pp}";
		}
	}
}
