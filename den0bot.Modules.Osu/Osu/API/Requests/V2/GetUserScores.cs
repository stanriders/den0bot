// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types.V2;

namespace den0bot.Modules.Osu.Osu.API.Requests.V2
{
	public class GetUserScores : IRequest
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"users/{username}/scores/{type}?include_fails={includeFails.ToString().ToLower()}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		private readonly string username;
		private readonly string type;
		private readonly bool includeFails;

		public GetUserScores(string username, ScoreType type, bool includeFails = false)
		{
			this.username = username;
			this.type = type.ToString().ToLower();
			this.includeFails = includeFails;
		}
	}

	public enum ScoreType
	{
		Best,
		Recent,
		First
	}
}