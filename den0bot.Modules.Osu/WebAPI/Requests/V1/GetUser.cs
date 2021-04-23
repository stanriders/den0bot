// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetUser : IRequest<List<Player>, Player>
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_user?u={username}";

		private readonly string username;

		public GetUser(string username)
		{
			this.username = username;
		}

		public Player Process(List<Player> data) => data[0];
	}
}
