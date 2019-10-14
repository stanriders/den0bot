using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	public class GetRecentScores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user_recent?limit={amount}&u={username}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		private string username;
		private int amount;

		public GetRecentScores(string username, int amount)
		{
			this.username = username;
			this.amount = amount;
		}
	}
}
