// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	class GetBeatmapSet : Request<BeatmapSet, BeatmapSet>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmapsets/{id}";

		private readonly uint id;

		public GetBeatmapSet(uint id)
		{
			this.id = id;
		}

		public override BeatmapSet? Process(BeatmapSet? data) => data;
	}
}
