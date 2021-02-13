// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetBeatmap : IRequest<List<Map>, Map>
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?limit=1&b={id}&mods={(int)(mods & LegacyMods.DifficultyChanging)}";

		private uint id;
		private LegacyMods mods;

		public GetBeatmap(uint id, LegacyMods mods = LegacyMods.NM)
		{
			this.id = id;
			this.mods = mods;
		}

		public Map Process(List<Map> data) => data[0];
	}
}
