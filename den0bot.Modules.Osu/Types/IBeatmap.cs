// den0bot (c) StanR 2021 - MIT License
using System;
using den0bot.Util;

namespace den0bot.Modules.Osu.Types
{
	public abstract class IBeatmap
	{
		public abstract uint Id { get; set; }
		public abstract uint BeatmapSetId { get; set; }
		public abstract string Version { get; set; }
		public abstract Mode Mode { get; set; }
		public abstract double AR { get; set; }
		public abstract double OD { get; set; }
		public abstract double CS { get; set; }
		public abstract double HP { get; set; }
		public abstract double BPM { get; set; }
		public abstract double StarRating { get; set; }
		public abstract uint Length { get; set; }
		public abstract uint DrainLength { get; set; }
		public abstract uint? MaxCombo { get; set; }
		public abstract uint? Circles { get; set; }
		public abstract uint? Sliders { get; set; }
		public abstract uint? Spinners { get; set; }
		public abstract bool Ranked { get; set; }
		public abstract RankedStatus Status { get; set; }
		public abstract string Thumbnail { get; }

		public abstract string GetFormattedMapInfo(LegacyMods mods);

		public string Link => "https://osu.ppy.sh/b/" + Id;
		public uint? ObjectsTotal => Circles + Sliders + Spinners;

		private string fileString = null;
		public string File
		{
			get
			{
				try
				{
					if (string.IsNullOrEmpty(fileString))
						fileString = Web.DownloadString("https://osu.ppy.sh/osu/" + Id).Result;

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
						fileBytes = Web.DownloadBytes("https://osu.ppy.sh/osu/" + Id).Result;
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

		public double ModdedBPM(LegacyMods mods)
		{
			if (mods.HasFlag(LegacyMods.DT) || mods.HasFlag(LegacyMods.NC))
			{
				return BPM * 1.5;
			}
			else if (mods.HasFlag(LegacyMods.HT))
			{
				return BPM * 0.75;
			}
			else
			{
				return BPM;
			}
		}

		public double ModdedCS(LegacyMods mods)
		{
			if (mods.HasFlag(LegacyMods.HR))
			{
				return CS * 1.3;
			}
			else if (mods.HasFlag(LegacyMods.EZ))
			{
				return CS * 0.5;
			}
			else
			{
				return CS;
			}
		}

		public double ModdedAR(LegacyMods mods)
		{
			double finalAR = AR;

			if (mods.HasFlag(LegacyMods.HR))
			{
				finalAR = Math.Min(finalAR * 1.4, 10);
			}
			else if (mods.HasFlag(LegacyMods.EZ))
			{
				finalAR *= 0.5;
			}

			double ms = (11700.0 - (900 * finalAR)) / 6.0;
			if (mods.HasFlag(LegacyMods.DT) || mods.HasFlag(LegacyMods.NC))
			{
				ms /= 1.5;
				finalAR = (11700.0 - (6 * ms)) / 900.0;
			}
			else if (mods.HasFlag(LegacyMods.HT))
			{
				ms /= 0.75;
				finalAR = (11700.0 - (6 * ms)) / 900.0;
			}

			return finalAR;
		}

		public double ModdedOD(LegacyMods mods)
		{
			double finalOD = OD;

			if (mods.HasFlag(LegacyMods.HR))
			{
				finalOD = Math.Min(finalOD * 1.4, 10);
			}
			else if (mods.HasFlag(LegacyMods.EZ))
			{
				finalOD *= 0.5;
			}

			double ms = 79.5 - (6 * finalOD);
			if (mods.HasFlag(LegacyMods.DT) || mods.HasFlag(LegacyMods.NC))
			{
				finalOD = (79.5 - (ms / 1.5)) / 6;
			}
			else if (mods.HasFlag(LegacyMods.HT))
			{
				finalOD = (79.5 - (ms / 0.75)) / 6;
			}
			return finalOD;
		}

		public double ModdedHP(LegacyMods mods)
		{
			if (mods.HasFlag(LegacyMods.HR))
			{
				return HP * 1.4;
			}
			else if (mods.HasFlag(LegacyMods.EZ))
			{
				return HP * 0.5;
			}
			else
			{
				return HP;
			}
		}

		public TimeSpan ModdedDrainLength(LegacyMods mods)
		{
			if (mods.HasFlag(LegacyMods.DT) || mods.HasFlag(LegacyMods.NC))
			{
				return TimeSpan.FromSeconds((long)(DrainLength * 0.6666666));
			}
			else if (mods.HasFlag(LegacyMods.HT))
			{
				return TimeSpan.FromSeconds((long)(DrainLength * 1.333333));
			}
			return TimeSpan.FromSeconds(DrainLength);
		}
	}
}
