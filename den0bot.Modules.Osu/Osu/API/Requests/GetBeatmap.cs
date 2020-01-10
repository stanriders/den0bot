// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	public class GetBeatmap : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?limit=1&b={id}&mods={(int)(mods & Mods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => true;

		private uint id;
		private Mods mods;

		public GetBeatmap(uint id, Mods mods = Mods.None)
		{
			this.id = id;
			this.mods = mods;
		}
	}
}
