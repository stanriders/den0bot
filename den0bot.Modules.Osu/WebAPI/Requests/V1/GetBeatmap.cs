// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetBeatmap : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?limit=1&b={id}&mods={(int)(mods & LegacyMods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => true;

		private uint id;
		private LegacyMods mods;

		public GetBeatmap(uint id, LegacyMods mods = LegacyMods.None)
		{
			this.id = id;
			this.mods = mods;
		}
	}
}
