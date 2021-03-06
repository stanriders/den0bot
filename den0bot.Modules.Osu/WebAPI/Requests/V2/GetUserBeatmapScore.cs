﻿// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.Util;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserBeatmapScore : IRequest<BeatmapUserScore, Score>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmaps/{beatmapId}/scores/users/{userId}{mods}";

		private readonly uint beatmapId;
		private readonly uint userId;
		private readonly string mods;

		public GetUserBeatmapScore(uint beatmapId, uint userId, LegacyMods? mods)
		{
			this.beatmapId = beatmapId;
			this.userId = userId;

			if (mods != null)
			{
				this.mods = "?mods[]=";

				var modsArray = mods.Value.ToArray(false);
				foreach (var mod in modsArray)
				{
					// this will produce incorrect request because of empty last mod but api allows it so whatever
					this.mods += mod + "&mods[]=";
				}
			}
		}

		public Score Process(BeatmapUserScore data) 
		{
			var score = data.Score;
			score.LeaderboardPosition = data.Position;
			return score;
		}
	}
}