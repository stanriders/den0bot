// den0bot (c) StanR 2021 - MIT License
using System;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapShort : IBeatmap
	{
		[JsonProperty("id")]
		public override uint Id { get; set; }

		[JsonProperty("beatmapset_id")]
		public override uint BeatmapSetId { get; set; }

		[JsonProperty("version")]
		public override string Version { get; set; }

		[JsonProperty("mode_int")]
		public override Mode Mode { get; set; }

		[JsonProperty("mode")]
		public string ModeName { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("ar")]
		public override double AR { get; set; }

		[JsonProperty("accuracy")]
		public override double OD { get; set; }

		[JsonProperty("cs")]
		public override double CS { get; set; }

		[JsonProperty("drain")]
		public override double HP { get; set; }

		[JsonProperty("bpm")]
		public override double BPM { get; set; }

		[JsonProperty("count_circles")]
		public override uint? Circles { get; set; }

		[JsonProperty("count_sliders")]
		public override uint? Sliders { get; set; }

		[JsonProperty("count_spinners")]
		public override uint? Spinners { get; set; }

		[JsonProperty("total_length")]
		public override uint Length { get; set; }

		[JsonProperty("hit_length")]
		public override uint DrainLength { get; set; }

		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
		public override RankedStatus Status { get; set; }

		[JsonProperty("difficulty_rating")]
		public override double StarRating { get; set; }

		[JsonProperty("ranked")]
		public override bool Ranked { get; set; }

		[JsonProperty("beatmapset")]
		public BeatmapSetShort BeatmapSet { get; set; }

		/*
            "convert": false,
            "deleted_at": null,
            "is_scoreable": true,
            "last_updated": "2019-10-04T00:29:36+00:00",
            "passcount": 277434,
            "playcount": 1058211,
		*/

		public override string Thumbnail => "https://assets.ppy.sh/beatmaps/" + BeatmapSetId + "/covers/card@2x.jpg";

		public override uint? MaxCombo { get; set; }

		public override string GetFormattedMapInfo(LegacyMods mods)
		{
			string pp = string.Empty;
			if (Mode == Mode.Osu)
			{
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
			}

			switch (Mode)
			{
				case Mode.Osu:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - <b>{Status}</b>\n" +
						$"⭕️ | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}\n" +
						$"{pp}";
				case Mode.Taiko:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - <b>{Status}</b>\n" +
						$"🥁 | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Fruits:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - <b>{Status}</b>\n" +
						$"🍎 | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Mania:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - <b>{Status}</b>\n" +
						$"🎹 | <b>Keys:</b> {CS:N0} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				default:
					return string.Empty;
			}
		}
	}

	public class Beatmap : BeatmapShort
	{
		[JsonProperty("max_combo")]
		public override uint? MaxCombo { get; set; }

		public override string Thumbnail => BeatmapSet.Covers.Cover2X;

		public override string GetFormattedMapInfo(LegacyMods mods)
		{
			string pp = string.Empty;
			if (Mode == Mode.Osu)
			{
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
			}

			switch (Mode)
			{
				case Mode.Osu:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {BeatmapSet.CreatorName} - <b>{Status}</b>\n" +
						$"⭕️ | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}\n" +
						$"{pp}";
				case Mode.Taiko:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {BeatmapSet.CreatorName} - <b>{Status}</b>\n" +
						$"🥁 | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Fruits:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {BeatmapSet.CreatorName} - <b>{Status}</b>\n" +
						$"🍎 | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Mania:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {BeatmapSet.CreatorName} - <b>{Status}</b>\n" +
						$"🎹 | <b>Keys:</b> {CS:N0} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				default:
					return string.Empty;
			}
		}
	}
}
