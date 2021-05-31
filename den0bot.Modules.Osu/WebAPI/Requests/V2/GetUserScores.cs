// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUserScores : IRequest<List<Score>, List<Score>>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"users/{username}/scores/{type}?include_fails={includeFails.ToString().ToLower()}&limit={limit}";

		private readonly string username;
		private readonly string type;
		private readonly bool includeFails;
		private readonly int limit;

		public GetUserScores(string username, ScoreType type, bool includeFails = false, int limit = 10)
		{
			this.username = username;
			this.type = type.ToString().ToLower();
			this.includeFails = includeFails;
			this.limit = limit;
		}

		public List<Score> Process(List<Score> data) => data;
	}

	public enum ScoreType
	{
		Best,
		Recent,
		First
	}
}