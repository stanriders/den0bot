// den0bot (c) StanR 2025 - MIT License
using osu.Game.Online.API.Requests.Responses;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetUser : Request<APIUser, APIUser>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"users/{username}";

		private readonly string username;

		public GetUser(string username)
		{
			this.username = username;
		}

		public override APIUser? Process(APIUser? data) => data;
	}
}
