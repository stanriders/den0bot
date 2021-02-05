// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUser : IRequest<User, User>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"users/{username}";

		private string username;

		public GetUser(string username)
		{
			this.username = username;
		}

		public User Process(User data) => data;
	}
}
