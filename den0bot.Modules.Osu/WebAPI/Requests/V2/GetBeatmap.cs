// den0bot (c) StanR 2020 - MIT License
using System;
using den0bot.Modules.Osu.Types.V2;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmap : IRequest
	{
		public APIVersion API => APIVersion.V2;

		public string Address => $"beatmaps/{id}";

		public Type ReturnType => typeof(Beatmap);

		public bool ShouldReturnSingle => true;

		private uint id;

		public GetBeatmap(uint id)
		{
			this.id = id;
		}
	}
}
