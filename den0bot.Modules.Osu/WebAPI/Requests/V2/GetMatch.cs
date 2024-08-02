// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetMatch : Request<Match, Match>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"matches/{matchId}";

		private readonly ulong matchId;

		public GetMatch(ulong matchId)
		{
			this.matchId = matchId;
		}

		public override Match? Process(Match? data) => data;
	}
}
