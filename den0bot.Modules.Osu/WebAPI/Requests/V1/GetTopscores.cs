// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetTopscores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user_best?limit={amount}&u={username}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		private string username;
		private int amount;

		public GetTopscores(string username, int amount)
		{
			this.username = username;
			this.amount = amount;
		}
	}
}
