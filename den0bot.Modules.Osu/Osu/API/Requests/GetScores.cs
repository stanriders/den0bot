using System;
using System.Collections.Generic;
using den0bot.Modules.Osu.Osu.Types;

namespace den0bot.Modules.Osu.Osu.API.Requests
{
	class GetScores : IRequest
	{
		public APIVersion API => APIVersion.V1;

		public string Address =>
			$"get_scores?b={BeatmapId}&u={Username}&limit={Amount}&mods={(int) (Mods & Mods.DifficultyChanging)}";

		public Type ReturnType => typeof(List<Score>);

		public bool ShouldReturnSingle => false;

		public string Username { get; set; }

		public uint BeatmapId { get; set; }

		public Mods Mods { get; set; }

		public int Amount { get; set; }
	}
}