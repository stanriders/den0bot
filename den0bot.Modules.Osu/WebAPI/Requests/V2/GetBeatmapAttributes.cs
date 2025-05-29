// den0bot (c) StanR 2024 - MIT License
using System.Linq;
using den0bot.Modules.Osu.Types.V2;
using Newtonsoft.Json;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmapAttributes : Request<DifficultyAttributes, DifficultyAttributes>
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

		public override DifficultyAttributes? Process(DifficultyAttributes? data)
		{
			return data;
		}
	}
}
