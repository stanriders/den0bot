// den0bot (c) StanR 2025 - MIT License
using System;
using System.Linq;
using den0bot.Util;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using Serilog;

namespace den0bot.Modules.Osu.Types.V2
{
	public class Beatmap : APIBeatmap
	{
		public string GetFormattedMapInfo(bool includeName = false)
		{
			return GetFormattedMapInfo([], includeName);
		}

		public string GetFormattedMapInfo(Mod[] mods, bool includeName = false)
		{
			string pp = string.Empty;
			var attributes = PpCalculation.CalculateDifficulty(mods, this);

			try
			{
				if (attributes != null)
				{
					double info100 = PpCalculation.CalculatePerformance(100, mods, attributes, this) ?? 0;
					if (info100 > 0)
					{
						pp = $"100% - {info100:N2}pp";

						double info98 = PpCalculation.CalculatePerformance(98, mods, attributes, this) ?? 0;
						if (info98 > 0)
							pp += $" | 98% - {info98:N2}pp";

						double info95 = PpCalculation.CalculatePerformance(95, mods, attributes, this) ?? 0;
						if (info95 > 0)
							pp += $" | 95% - {info95:N2}pp";
					}
				}
			}
			catch (Exception e)
			{
				Log.Error($"PP failed: {e.InnerMessageIfAny()}");
			}

			var fullName = string.Empty;
			if (includeName)
			{
				fullName = $"<a href=\"{Link}\">{BeatmapSet?.Artist} - {BeatmapSet?.Title}</a>\n";
			}

			var apiMods = mods.Select(x => new APIMod(x)).ToArray();

			switch (Ruleset.ShortName)
			{
				case "osu":
					return
						$"{fullName}" +
						$"[{DifficultyName.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.AuthorString} - <b>{Status}</b>\n" +
						$"⭕️ | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(apiMods):N2} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case "taiko":
					return
						$"{fullName}" +
						$"[{DifficultyName.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.AuthorString} - <b>{Status}</b>\n" +
						$"🥁 | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case "catch:":
					return
						$"{fullName}" +
						$"[{DifficultyName.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.AuthorString} - <b>{Status}</b>\n" +
						$"🍎 | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(apiMods):N2} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case "mania":
					return
						$"{fullName}" +
						$"[{DifficultyName.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.AuthorString} - <b>{Status}</b>\n" +
						$"🎹 | <b>Keys:</b> {CircleSize:N0} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				default:
					return string.Empty;
			}
		}

		public string Link => "https://osu.ppy.sh/beatmaps/" + OnlineID;

		private byte[]? fileBytes = null;
		public byte[]? FileBytes
		{
			get
			{
				try
				{
					if (fileBytes == null)
					{
						fileBytes = Web.DownloadBytes("https://osu.ppy.sh/osu/" + OnlineID).Result;
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
		public double ModdedBPM(APIMod[] mods)
		{
			if (mods.Any(x=> x.Acronym is "DT" or "NC") /*|| mods.HasFlag(LegacyMods.NC))*/)
			{
				var dt = mods.FirstOrDefault(x => x.Acronym is "DT" or "NC");
				if (dt?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)dt.Settings["speed_change"]; 
					return BPM * speedChange;
				}
				
				return BPM * 1.5;
			}
			else if (mods.Any(x => x.Acronym == "HT") )
			{
				var ht = mods.FirstOrDefault(x => x.Acronym == "HT");
				if (ht?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)ht.Settings["speed_change"];
					return BPM * speedChange;
				}

				return BPM * 0.75;
			}
			else
			{
				return BPM;
			}
		}

		public double ModdedCS(Mod[] mods)
		{
			if (mods.Any(x => x.Acronym == "HR"))
			{
				return CircleSize * 1.3;
			}
			else if (mods.Any(x => x.Acronym == "EZ"))
			{
				return CircleSize * 0.5;
			}
			else
			{
				return CircleSize;
			}
		}

		public double ModdedAR(APIMod[] mods)
		{
			double finalAR = ApproachRate;

			if (mods.Any(x => x.Acronym == "HR"))
			{
				finalAR = Math.Min(finalAR * 1.4, 10);
			}
			else if (mods.Any(x => x.Acronym == "EZ"))
			{
				finalAR *= 0.5;
			}

			double ms = (11700.0 - (900 * finalAR)) / 6.0;
			if (mods.Any(x => x.Acronym is "DT" or "NC"))
			{
				var dt = mods.FirstOrDefault(x => x.Acronym is "DT" or "NC");
				if (dt?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)dt.Settings["speed_change"];
					ms /= speedChange;
				}
				else
				{
					ms /= 1.5;
				}

				finalAR = (11700.0 - (6 * ms)) / 900.0;
			}
			else if (mods.Any(x => x.Acronym == "HT"))
			{
				var ht = mods.FirstOrDefault(x => x.Acronym == "HT");
				if (ht?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)ht.Settings["speed_change"];
					ms /= speedChange;
				}
				else
				{
					ms /= 0.75;
				}

				finalAR = (11700.0 - (6 * ms)) / 900.0;
			}

			return finalAR;
		}

		public double ModdedOD(APIMod[] mods)
		{
			double finalOD = OverallDifficulty;

			if (mods.Any(x => x.Acronym == "HR"))
			{
				finalOD = Math.Min(finalOD * 1.4, 10);
			}
			else if (mods.Any(x => x.Acronym == "EZ"))
			{
				finalOD *= 0.5;
			}

			double ms = 79.5 - (6 * finalOD);
			if (mods.Any(x => x.Acronym is "DT" or "NC"))
			{
				var dt = mods.FirstOrDefault(x => x.Acronym is "DT" or "NC");
				if (dt?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)dt.Settings["speed_change"];
					finalOD = (79.5 - ms / speedChange) / 6;
				}
				else
				{
					finalOD = (79.5 - (ms / 1.5)) / 6;
				}
			}
			else if (mods.Any(x => x.Acronym == "HT"))
			{
				var ht = mods.FirstOrDefault(x => x.Acronym == "HT");
				if (ht?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)ht.Settings["speed_change"];
					finalOD = (79.5 - ms / speedChange) / 6;
				}
				else
				{
					finalOD = (79.5 - (ms / 0.75)) / 6;
				}
			}
			return finalOD;
		}

		public TimeSpan ModdedDrainLength(APIMod[] mods)
		{
			if (mods.Any(x => x.Acronym is "DT" or "NC"))
			{
				var dt = mods.FirstOrDefault(x => x.Acronym is "DT" or "NC");
				if (dt?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)dt.Settings["speed_change"];
					return TimeSpan.FromSeconds((long)(Length / speedChange));
				}

				return TimeSpan.FromSeconds((long)(Length / 1.5));
			}

			if (mods.Any(x => x.Acronym == "HT"))
			{
				var ht = mods.FirstOrDefault(x => x.Acronym == "HT");
				if (ht?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)ht.Settings["speed_change"];
					return TimeSpan.FromSeconds((long)(Length / speedChange));
				}

				return TimeSpan.FromSeconds((long)(Length / 0.75));
			}

			return TimeSpan.FromSeconds(Length);
		}
	}
}
