// den0bot (c) StanR 2024 - MIT License
namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapSetSearchResult
	{
		public BeatmapSet[] BeatmapSets { get; set; } = null!;

		public int Total { get; set; }
	}
}
