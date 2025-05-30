// den0bot (c) StanR 2025 - MIT License
using osu.Game.Online.API.Requests.Responses;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	class GetBeatmapSet : Request<APIBeatmapSet, APIBeatmapSet>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmapsets/{id}";

		private readonly int id;

		public GetBeatmapSet(int id)
		{
			this.id = id;
		}

		public override APIBeatmapSet? Process(APIBeatmapSet? data) => data;
	}
}
