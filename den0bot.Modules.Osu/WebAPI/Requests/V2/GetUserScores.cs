// den0bot (c) StanR 2025 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserScores : Request<List<Score>, List<Score>>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"users/{userId}/scores/{type}?include_fails={includeFails.ToString().ToLower()}&limit={limit}";

		private readonly int userId;
		private readonly string type;
		private readonly bool includeFails;
		private readonly int limit;

		public GetUserScores(int userId, ScoreType type, bool includeFails = false, int limit = 10)
		{
			this.userId = userId;
			this.type = type.ToString().ToLower();
			this.includeFails = includeFails;
			this.limit = limit;
		}

		public override List<Score>? Process(List<Score>? data) => data;
	}

	public enum ScoreType
	{
		Best,
		Recent,
		First
	}
}