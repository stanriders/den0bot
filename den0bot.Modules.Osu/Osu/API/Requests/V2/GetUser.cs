// den0bot (c) StanR 2020 - MIT License
using System;
using den0bot.Modules.Osu.Osu.Types.V2;

namespace den0bot.Modules.Osu.Osu.API.Requests.V2
{
	public class GetUser : IRequest
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"users/{username}";

		public Type ReturnType => typeof(User);

		public bool ShouldReturnSingle => true;

		private string username;

		public GetUser(string username)
		{
			this.username = username;
		}
	}
}
