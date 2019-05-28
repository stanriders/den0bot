using System;
using System.Collections.Generic;
using den0bot.Osu.Types;

namespace den0bot.Osu.API.Requests
{
	class GetRecentScores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user_recent?limit={Amount}&u={Username}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		public string Username { get; set; }

		public int Amount { get; set; }
	}
}
