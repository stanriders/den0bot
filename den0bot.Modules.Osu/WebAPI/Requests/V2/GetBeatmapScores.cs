// den0bot (c) StanR 2023 - MIT License
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.Util;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmapScores : Request<BeatmapScores, Score[]>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{beatmapId}/scores";

		private readonly uint beatmapId;
		//private readonly string mods;

		public GetBeatmapScores(uint beatmapId/*, LegacyMods? mods*/)
		{
			this.beatmapId = beatmapId;

			// Mod querying is locked for API for now.
			/*
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
			*/
		}

		public override Score[] Process(BeatmapScores data) => data.Scores;
	}
}
