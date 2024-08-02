// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmap : Request<Beatmap, Beatmap>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{id}";

		private readonly uint id;

		public GetBeatmap(uint id)
		{
			this.id = id;
		}

		public override Beatmap? Process(Beatmap? data) => data;
	}
}
