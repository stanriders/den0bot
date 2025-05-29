// den0bot (c) StanR 2025 - MIT License
using System;
using System.Linq;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using osu.Game.Online.API;
using osu.Game.Rulesets.Mods;
using Serilog;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapShort
	{
		[JsonProperty("id")]
		public uint Id { get; set; }

		[JsonProperty("beatmapset_id")]
		public uint BeatmapSetId { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; } = null!;

		[JsonProperty("mode_int")]
		public Mode Mode { get; set; }

		[JsonProperty("mode")]
		public string ModeName { get; set; } = null!;

		[JsonProperty("url")]
		public string Url { get; set; } = null!;

		[JsonProperty("ar")]
		public double AR { get; set; }

		[JsonProperty("accuracy")]
		public double OD { get; set; }

		[JsonProperty("cs")]
		public double CS { get; set; }

		[JsonProperty("drain")]
		public double HP { get; set; }

		[JsonProperty("bpm")]
		public double BPM { get; set; }

		[JsonProperty("count_circles")]
		public int Circles { get; set; }

		[JsonProperty("count_sliders")]
		public int Sliders { get; set; }

		[JsonProperty("count_spinners")]
		public int Spinners { get; set; }

		[JsonProperty("total_length")]
		public int Length { get; set; }

		[JsonProperty("hit_length")]
		public int DrainLength { get; set; }

		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
		public RankedStatus Status { get; set; }

		[JsonProperty("difficulty_rating")]
		public double StarRating { get; set; }

		[JsonProperty("ranked")]
		public bool Ranked { get; set; } 

		[JsonProperty("beatmapset")]
		public BeatmapSetShort? BeatmapSet { get; set; }

		/*
            "convert": false,
            "deleted_at": null,
            "is_scoreable": true,
            "last_updated": "2019-10-04T00:29:36+00:00",
            "passcount": 277434,
            "playcount": 1058211,
		*/

		public string Link => "https://osu.ppy.sh/b/" + Id;
		public int? ObjectsTotal => Circles + Sliders + Spinners;

		private byte[]? fileBytes = null;
		public byte[]? FileBytes
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
				return CS * 1.3;
			}
			else if (mods.Any(x => x.Acronym == "EZ"))
			{
				return CS * 0.5;
			}
			else
			{
				return CS;
			}
		}

		public double ModdedAR(APIMod[] mods)
		{
			double finalAR = AR;

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
			double finalOD = OD;

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
					return TimeSpan.FromSeconds((long)(DrainLength / speedChange));
				}

				return TimeSpan.FromSeconds((long)(DrainLength / 1.5));
			}

			if (mods.Any(x => x.Acronym == "HT"))
			{
				var ht = mods.FirstOrDefault(x => x.Acronym == "HT");
				if (ht?.Settings?.ContainsKey("speed_change") ?? false)
				{
					var speedChange = (double)ht.Settings["speed_change"];
					return TimeSpan.FromSeconds((long)(DrainLength / speedChange));
				}

				return TimeSpan.FromSeconds((long)(DrainLength / 0.75));
			}

			return TimeSpan.FromSeconds(DrainLength);
		}
	}

	public class Beatmap : BeatmapShort
	{
		[JsonProperty("max_combo")]
		public int MaxCombo { get; set; }

		public string? Thumbnail => BeatmapSet?.Covers?.Cover2X;

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

			switch (Mode)
			{
				case Mode.Osu:
					return
						$"{fullName}" +
						$"[{Version.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.CreatorName} - <b>{Status}</b>\n" +
						$"⭕️ | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(apiMods):N2} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case Mode.Taiko:
					return
						$"{fullName}" +
						$"[{Version.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.CreatorName} - <b>{Status}</b>\n" +
						$"🥁 | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case Mode.Fruits:
					return
						$"{fullName}" +
						$"[{Version.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.CreatorName} - <b>{Status}</b>\n" +
						$"🍎 | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(apiMods):N2} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				case Mode.Mania:
					return
						$"{fullName}" +
						$"[{Version.FilterToHTML()}] - {attributes?.StarRating:N2}* - {ModdedDrainLength(apiMods):mm\':\'ss} - {BeatmapSet?.CreatorName} - <b>{Status}</b>\n" +
						$"🎹 | <b>Keys:</b> {CS:N0} | <b>OD:</b> {ModdedOD(apiMods):N2} | <b>BPM:</b> {ModdedBPM(apiMods):N2}\n" +
						$"{pp}";
				default:
					return string.Empty;
			}
		}
	}
}
