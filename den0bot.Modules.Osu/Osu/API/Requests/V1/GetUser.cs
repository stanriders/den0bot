// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types.V1;

namespace den0bot.Modules.Osu.Osu.API.Requests.V1
{
	public class GetUser : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user?u={username}";

		public Type ReturnType => typeof(List<Player>);

		public bool ShouldReturnSingle => true;

		private string username;

		public GetUser(string username)
		{
			this.username = username;
		}
	}
}
