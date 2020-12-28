﻿// den0bot (c) StanR 2020 - MIT License
using System;
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	class GetBeatmapSet : IRequest
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmapsets/{id}";

		public Type ReturnType => typeof(BeatmapSet);

		public bool ShouldReturnSingle => true;

		private uint id;

		public GetBeatmapSet(uint id)
		{
			this.id = id;
		}
	}
}