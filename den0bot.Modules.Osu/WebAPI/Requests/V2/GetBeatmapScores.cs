// den0bot (c) StanR 2024 - MIT License
using den0bot.Modules.Osu.Types.V2;
using osu.Game.Rulesets.Mods;

namespace den0bot.Modules.Osu.WebAPI.Requests.V2
{
	public class GetBeatmapScores : Request<BeatmapScores, Score[]>
	{
		public override APIVersion API => APIVersion.V2;

		public override string Address => $"beatmaps/{beatmapId}/scores{mods}";

		private readonly uint beatmapId;
		private readonly string mods = string.Empty;

		public GetBeatmapScores(uint beatmapId, Mod[]? mods)
		{
			this.beatmapId = beatmapId;
			
			if (mods != null)
			{
				this.mods = "?mods[]=";
				
				foreach (var mod in mods)
				{
					// this will produce incorrect request because of empty last mod but api allows it so whatever
					this.mods += mod.Acronym + "&mods[]=";
				}
			}
			
		}

		public override Score[]? Process(BeatmapScores? data) => data?.Scores;
	}
}
