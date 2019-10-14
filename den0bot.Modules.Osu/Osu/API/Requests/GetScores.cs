using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	public class GetScores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address =>
			$"get_scores?b={beatmapId}&u={username}&limit={amount}{(mods != Mods.None ? "&mods=" + (int)mods : "")}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		private string username;
		private uint beatmapId;
		private Mods mods;
		private int amount;

		public GetScores(string username, uint beatmapId, Mods mods, int amount)
		{
			this.username = username;
			this.beatmapId = beatmapId;
			this.mods = mods;
			this.amount = amount;
		}
	}
}