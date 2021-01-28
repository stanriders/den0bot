// den0bot (c) StanR 2021 - MIT License
using System;
using den0bot.Util;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V1
{
	public class Map : IBeatmap
	{
		[JsonProperty("beatmap_id")]
		public override uint Id { get; set; }
		[JsonProperty("beatmapset_id")]
		public override uint BeatmapSetId { get; set; }

		[JsonProperty("last_update")]
		public DateTime? UpdatedDate;
		[JsonProperty("approved_date")]
		public DateTime? RankedDate;
		[JsonProperty("approved")]
		public override RankedStatus Status { get; set; }

		[JsonProperty("artist")]
		public string Artist;
		[JsonProperty("title")]
		public string Title;
		[JsonProperty("version")]
		public override string Version { get; set; }
		[JsonProperty("creator")]
		public string Creator;
		[JsonProperty("mode")]
		public override Mode Mode { get; set; }

		[JsonProperty("difficultyrating")]
		public override double StarRating { get; set; }
		[JsonProperty("diff_size")]
		public override double CS { get; set; }
		[JsonProperty("diff_approach")]
		public override double AR { get; set; }
		[JsonProperty("diff_overall")]
		public override double OD { get; set; }
		[JsonProperty("diff_drain")]
		public override double HP { get; set; }

		[JsonProperty("max_combo")]
		public override uint? MaxCombo { get; set; }
		[JsonProperty("hit_length")]
		public override uint DrainLength { get; set; }
		[JsonProperty("total_length")]
		public override uint Length { get; set; }
		[JsonProperty("count_normal")]
		public override uint? Circles { get; set; }
		[JsonProperty("count_slider")]
		public override uint? Sliders { get; set; }
		[JsonProperty("count_spinner")]
		public override uint? Spinners { get; set; }

		[JsonProperty("bpm")]
		public override double BPM { get; set; }

		public override bool Ranked
		{
			get => Status == RankedStatus.Ranked || Status == RankedStatus.Approved;
			set { }
		}

		public override string Thumbnail => "https://assets.ppy.sh/beatmaps/" + BeatmapSetId + "/covers/card@2x.jpg";

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
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {Creator} - <b>{Status}</b>\n" +
						$"⭕️ | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}\n" +
						$"{pp}";
				case Mode.Taiko:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {Creator} - <b>{Status}</b>\n" +
						$"🥁 | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Fruits:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {Creator} - <b>{Status}</b>\n" +
						$"🍎 | <b>CS:</b> {ModdedCS(mods):N2} | <b>AR:</b> {ModdedAR(mods):N2} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				case Mode.Mania:
					return
						$"[{Version.FilterToHTML()}] - {StarRating:N2}* - {ModdedDrainLength(mods):mm\':\'ss} - {Creator} - <b>{Status}</b>\n" +
						$"🎹 | <b>Keys:</b> {CS:N0} | <b>OD:</b> {ModdedOD(mods):N2} | <b>BPM:</b> {ModdedBPM(mods):N2}";
				default:
					return string.Empty;
			}
		}
	}
}
