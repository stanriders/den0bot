// den0bot (c) StanR 2023 - MIT License
using System.Collections.Generic;
using System.Linq;
using Pettanko;
using den0bot.Modules.Osu.Types.V2;
using Pettanko.Difficulty;

namespace den0bot.Modules.Osu
{
	public class PpCalculation
	{
		public static double CalculatePerformance(double accuracy, string[] mods, OsuDifficultyAttributes difficultyAttributes, Beatmap beatmap)
		{
			// TEMP
			var score = new Types.V2.Score
			{
				Accuracy = accuracy / 100.0,
				Combo = difficultyAttributes.MaxCombo,
				Mods = mods,
				Statistics = new Types.V2.Score.ScoreStatistics
				{
					Count300 = beatmap.ObjectsTotal ?? 0
				}
			};
			return CalculatePerformance(score, difficultyAttributes, beatmap);
		}

		public static double CalculatePerformance(Types.V2.Score score, OsuDifficultyAttributes difficultyAttributes, Beatmap beatmap)
		{
			difficultyAttributes.HitCircleCount = beatmap.Circles;
			difficultyAttributes.SliderCount = beatmap.Sliders;
			difficultyAttributes.SpinnerCount = beatmap.Spinners;

			var mods = new List<Mod>();
			foreach (var mod in score.Mods)
			{
				var pettankoMod = Mod.AllMods.FirstOrDefault(x => x.Acronym == mod);
				if (pettankoMod != null)
					mods.Add(pettankoMod);
			}

			var perfAttributes = Pettanko.Pettanko.Calculate(difficultyAttributes, new Pettanko.Score
			{
				Accuracy = score.Accuracy / 100.0,
				MaxCombo = score.Combo,
				RulesetId = 0,
				Statistics = new Statistics
				{
					Count300 = score.Statistics.Count300,
					Count100 = score.Statistics.Count100,
					Count50 = score.Statistics.Count50,
					CountMiss = score.Statistics.CountMiss
				},
				Mods = mods.ToArray()
			});

			return perfAttributes.Total;
		}
	}
}
