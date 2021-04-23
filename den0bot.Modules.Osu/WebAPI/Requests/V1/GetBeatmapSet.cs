// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V1;

namespace den0bot.Modules.Osu.WebAPI.Requests.V1
{
	public class GetBeatmapSet : IRequest<List<Map>, List<Map>>
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?s={id}&mods={(int)(mods & LegacyMods.DifficultyChanging)}";

		private readonly uint id;
		private readonly LegacyMods mods;

		public GetBeatmapSet(uint id, LegacyMods mods = LegacyMods.NM)
		{
			this.id = id;
			this.mods = mods;
		}

		public List<Map> Process(List<Map> data) => data;
	}
}
