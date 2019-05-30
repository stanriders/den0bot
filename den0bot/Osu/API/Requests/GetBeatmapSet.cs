﻿using System;
using System.Collections.Generic;
using den0bot.Osu.Types;

namespace den0bot.Osu.API.Requests
{
	class GetBeatmapSet : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_beatmaps?s={ID}&mods={(int)(Mods & Mods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Map>);

		public bool ShouldReturnSingle => false;

		public uint ID { get; set; }

		public Mods Mods { get; set; }

	}
}
