// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUser : Request<User, User>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"users/{username}";

		private readonly string username;

		public GetUser(string username)
		{
			this.username = username;
		}

		public override User? Process(User? data) => data;
	}
}
