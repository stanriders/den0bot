using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	public class GetBeatmapSet : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?s={id}&mods={(int)(mods & Mods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => false;

		private uint id;
		private Mods mods;

		public GetBeatmapSet(uint id, Mods mods = Mods.None)
		{
			this.id = id;
			this.mods = mods;
		}
	}
}
