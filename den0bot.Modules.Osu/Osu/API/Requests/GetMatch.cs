﻿using System;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	class GetMatch : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address => $"get_match?mp={ID}";

		public Type ReturnType => typeof(MultiplayerMatch);

		public bool ShouldReturnSingle => false;

		public ulong ID { get; set; }
	}
}
