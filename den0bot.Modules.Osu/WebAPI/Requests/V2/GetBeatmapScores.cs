// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.Util;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmapScores : IRequest<BeatmapScores, Score[]>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmaps/{beatmapId}/scores{mods}";

		private readonly uint beatmapId;
		private readonly string mods;

		public GetBeatmapScores(uint beatmapId, LegacyMods? mods)
		{
			this.beatmapId = beatmapId;

			if (mods != null)
			{
				this.mods = "?mods[]=";

				var modsArray = mods.Value.ToArray();
				foreach (var mod in modsArray)
				{
					// this will produce incorrect request because of empty last mod but api allows it so whatever
					this.mods += mod + "&mods[]=";
				}
			}
		}

		public Score[] Process(BeatmapScores data) => data.Scores;
	}
}
