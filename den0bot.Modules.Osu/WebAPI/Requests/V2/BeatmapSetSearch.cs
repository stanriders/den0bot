// den0bot (c) StanR 2025 - MIT License
using den0bot.Modules.Osu.Types.V2;
using osu.Game.Online.API.Requests.Responses;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class BeatmapSetSearch : Request<BeatmapSetSearchResult, APIBeatmapSet[]>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmapsets/search?q={query}&s={(ranked ? "leaderboard" : "any")}";

		private readonly string query;
		private readonly bool ranked;

		public BeatmapSetSearch(string query, bool ranked = false)
		{
			this.query = query;
			this.ranked = ranked;
		}

		public override APIBeatmapSet[]? Process(BeatmapSetSearchResult? data) => data?.BeatmapSets;
	}
}
