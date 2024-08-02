// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class BeatmapSetSearch : Request<BeatmapSetSearchResult, BeatmapSet[]>
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

		public override BeatmapSet[]? Process(BeatmapSetSearchResult? data) => data?.BeatmapSets;
	}
}
