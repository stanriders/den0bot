// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetMatch : IRequest<MultiplayerMatch, MultiplayerMatch>
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_match?mp={id}";

		private ulong id;

		public GetMatch(ulong id)
		{
			this.id = id;
		}

		public MultiplayerMatch Process(MultiplayerMatch data) => data;
	}
}
