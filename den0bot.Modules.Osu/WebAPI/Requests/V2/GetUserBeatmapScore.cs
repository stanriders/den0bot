// den0bot (c) StanR 2025 - MIT License

using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserBeatmapScore : Request<BeatmapUserScore, Score>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{beatmapId}/scores/users/{userId}{mods}";

		private readonly int beatmapId;
		private readonly uint userId;
		private readonly string mods = string.Empty;

		public GetUserBeatmapScore(int beatmapId, uint userId, string[] mods)
		{
			this.beatmapId = beatmapId;
			this.userId = userId;

			if (mods is { Length: > 0 })
			{
				this.mods = "?mods[]=" + string.Join("&mods[]=", mods);
			}
		}

		public override Score? Process(BeatmapUserScore? data) 
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