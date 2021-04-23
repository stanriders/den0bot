// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	class GetBeatmapSet : IRequest<BeatmapSet, BeatmapSet>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmapsets/{id}";

		private readonly uint id;

		public GetBeatmapSet(uint id)
		{
			this.id = id;
		}

		public BeatmapSet Process(BeatmapSet data) => data;
	}
}
