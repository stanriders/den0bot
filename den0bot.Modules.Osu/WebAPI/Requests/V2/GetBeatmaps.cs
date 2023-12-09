// den0bot (c) StanR 2023 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmaps : Request<BeatmapsBatch, Beatmap[]>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps?ids[]={string.Join("ids[]=", ids)}";

		private readonly uint[] ids;

		public GetBeatmaps(uint[] ids)
		{
			this.ids = ids;
		}

		public override Beatmap[] Process(BeatmapsBatch data) => data.Beatmaps;
	}
}
