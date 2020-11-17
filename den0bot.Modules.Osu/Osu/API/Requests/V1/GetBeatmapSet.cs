// den0bot (c) StanR 2020 - MIT License

using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Modules.Osu.Osu.Types.V1;

namespace den0bot.Modules.Osu.Osu.API.Requests.V1
{
	public class GetBeatmapSet : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?s={id}&mods={(int)(mods & LegacyMods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => false;

		private uint id;
		private LegacyMods mods;

		public GetBeatmapSet(uint id, LegacyMods mods = LegacyMods.None)
		{
			this.id = id;
			this.mods = mods;
		}
	}
}
