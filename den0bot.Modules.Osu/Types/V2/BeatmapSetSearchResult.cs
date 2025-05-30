// den0bot (c) StanR 2025 - MIT License
using osu.Game.Online.API.Requests.Responses;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapSetSearchResult
	{
		public APIBeatmapSet[] BeatmapSets { get; set; } = null!;

		public int Total { get; set; }
	}
}
