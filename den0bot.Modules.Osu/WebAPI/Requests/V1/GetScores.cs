﻿// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetScores : IRequest<List<Score>, List<Score>>
	{
		public APIVersion API => APIVersion.V1;

		public string Address =>
			$"get_scores?b={beatmapId}&u={username}&limit={amount}{(mods != LegacyMods.NM ? "&mods=" + (int)mods : "")}";

		private readonly string username;
		private readonly uint beatmapId;
		private readonly LegacyMods mods;
		private readonly int amount;

		public GetScores(string username, uint beatmapId, LegacyMods mods, int amount)
		{
			this.username = username;
			this.beatmapId = beatmapId;
			this.mods = mods;
			this.amount = amount;
		}

		public List<Score> Process(List<Score> data) => data;
	}
}