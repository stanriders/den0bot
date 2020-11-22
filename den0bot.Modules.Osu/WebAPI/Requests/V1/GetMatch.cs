// den0bot (c) StanR 2020 - MIT License

using System;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetMatch : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_match?mp={id}";

		public Type ReturnType => typeof(MultiplayerMatch);

		public bool ShouldReturnSingle => false;

		private ulong id;

		public GetMatch(ulong id)
		{
			this.id = id;
		}
	}
}
