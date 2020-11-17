// den0bot (c) StanR 2020 - MIT License
using System;
using den0bot.Util;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types.V2
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
		public string Status { get; set; }

		[JsonProperty("difficulty_rating")]
		public override double StarRating { get; set; }

		[JsonProperty("ranked")]
		public override bool Ranked { get; set; }

		/*
            "convert": false,
            "deleted_at": null,
            "is_scoreable": true,
            "last_updated": "2019-10-04T00:29:36+00:00",
            "passcount": 277434,
            "playcount": 1058211,
		*/

		public override string Thumbnail => "https://assets.ppy.sh/beatmaps/" + BeatmapSetId + "/covers/card@2x.jpg";

		private uint? maxCombo;
		public override uint? MaxCombo 
		{ 
			get => maxCombo; 
			set { maxCombo = value; } 
		}

		public override string GetFormattedMapInfo(LegacyMods mods)
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
				$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - <b>{Status.Capitalize()}</b>\n" +
				$"<b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}\n" +
				$"{pp}";
		}
	}
	public class Beatmap : BeatmapShort
	{
		[JsonProperty("beatmapset")]
		public BeatmapSetShort BeatmapSet { get; set; }

		[JsonProperty("max_combo")]
		public override uint? MaxCombo { get; set; }

		public override string Thumbnail => BeatmapSet.Covers.Cover2X;

		public override string GetFormattedMapInfo(LegacyMods mods)
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
				$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {BeatmapSet.CreatorName} - <b>{Status.Capitalize()}</b>\n" +
				$"<b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}\n" +
				$"{pp}";
		}
	}
}
