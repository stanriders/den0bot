// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmap : IRequest<Beatmap, Beatmap>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmaps/{id}";

		private uint id;

		public GetBeatmap(uint id)
		{
			this.id = id;
		}

		public Beatmap Process(Beatmap data) => data;
	}
}
