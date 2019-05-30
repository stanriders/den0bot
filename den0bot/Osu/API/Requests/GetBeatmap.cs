using System;
using System.Collections.Generic;
using den0bot.Osu.Types;

namespace den0bot.Osu.API.Requests
{
	class GetBeatmap : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?limit=1&b={ID}&mods={(int)(Mods & Mods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => true;

		public uint ID { get; set; }

		public Mods Mods { get; set; }
	}
}
