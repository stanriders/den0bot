// den0bot (c) StanR 2024 - MIT License
using System.Linq;
using den0bot.Modules.Osu.Types.V2;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmapAttributes : Request<DifficultyAttributes, Pettanko.Difficulty.OsuDifficultyAttributes>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{id}/attributes";

		public override string Body => $"{{\"mods\": {JsonConvert.SerializeObject(mods.Select(x=> x.Acronym))}}}";

		private readonly uint id;
		private readonly Mod[] mods;

		public GetBeatmapAttributes(uint id, Mod[] mods)
		{
			this.id = id;
			this.mods = mods;
		}

		public override Pettanko.Difficulty.OsuDifficultyAttributes Process(DifficultyAttributes data)
		{
			return new Pettanko.Difficulty.OsuDifficultyAttributes
			{
				StarRating = data.Attributes.StarRating,
				MaxCombo = data.Attributes.MaxCombo,
				AimDifficulty = data.Attributes.AimDifficulty,
				ApproachRate = data.Attributes.ApproachRate,
				FlashlightDifficulty = data.Attributes.FlashlightDifficulty,
				OverallDifficulty = data.Attributes.OverallDifficulty,
				SliderFactor = data.Attributes.SliderFactor,
				SpeedDifficulty = data.Attributes.SpeedDifficulty
			};
		}
	}
}
