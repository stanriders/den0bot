// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetMatch : IRequest<Match, Match>
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"matches/{matchId}";

		private readonly ulong matchId;

		public GetMatch(ulong matchId)
		{
			this.matchId = matchId;
		}

		public Match Process(Match data) => data;
	}
}
