// den0bot (c) StanR 2024 - MIT License

using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserBeatmapScore : Request<BeatmapUserScore, LazerScore>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{beatmapId}/scores/users/{userId}{mods}";

		private readonly uint beatmapId;
		private readonly uint userId;
		private readonly string mods = string.Empty;

		public GetUserBeatmapScore(uint beatmapId, uint userId, string[] mods)
		{
			this.beatmapId = beatmapId;
			this.userId = userId;

			if (mods is { Length: > 0 })
			{
				this.mods = "?mods[]=" + string.Join("&mods[]=", mods);
			}
		}

		public override LazerScore? Process(BeatmapUserScore? data) 
		{
			var score = data?.Score;
			if (score == null)
			{
				return null;
			}

			score.LeaderboardPosition = data?.Position;
			return score;
		}
	}
}