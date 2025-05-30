// den0bot (c) StanR 2024 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserBeatmapScores : Request<BeatmapUserScores, List<Score>>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{beatmapId}/scores/users/{userId}/all";

		private readonly uint beatmapId;
		private readonly uint userId;

		public GetUserBeatmapScores(uint beatmapId, uint userId)
		{
			this.beatmapId = beatmapId;
			this.userId = userId;
		}

		public override List<Score>? Process(BeatmapUserScores? data)
		{
			return data?.Scores;
		}
	}
}