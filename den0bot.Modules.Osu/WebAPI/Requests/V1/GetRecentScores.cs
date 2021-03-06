﻿// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetRecentScores : IRequest<List<Score>, List<Score>>
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user_recent?limit={amount}&u={username}";

		private readonly string username;
		private readonly int amount;

		public GetRecentScores(string username, int amount)
		{
			this.username = username;
			this.amount = amount;
		}

		public List<Score> Process(List<Score> data) => data;
	}
}
