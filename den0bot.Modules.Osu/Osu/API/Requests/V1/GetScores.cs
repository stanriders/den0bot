// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Modules.Osu.Osu.Types.V1;

namespace den0bot.Modules.Osu.Osu.API.Requests.V1
{
	public class GetScores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address =>
			$"get_scores?b={beatmapId}&u={username}&limit={amount}{(mods != LegacyMods.None ? "&mods=" + (int)mods : "")}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		private string username;
		private uint beatmapId;
		private LegacyMods mods;
		private int amount;

		public GetScores(string username, uint beatmapId, LegacyMods mods, int amount)
		{
			this.username = username;
			this.beatmapId = beatmapId;
			this.mods = mods;
			this.amount = amount;
		}
	}
}