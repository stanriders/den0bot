// den0bot (c) StanR 2025 - MIT License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using Beatmap = den0bot.Modules.Osu.Types.V2.Beatmap;
using DifficultyAttributes = osu.Game.Rulesets.Difficulty.DifficultyAttributes;

namespace den0bot.Modules.Osu
{
	public static class PpCalculation
	{
		public static readonly OsuRuleset Osu = new();
		public static readonly TaikoRuleset Taiko = new();
		public static readonly CatchRuleset Catch = new();
		public static readonly ManiaRuleset Mania = new();

		public static DifficultyAttributes? CalculateDifficulty(Mod[] mods, Beatmap beatmap)
		{
			var ruleset = GetRuleset((int)beatmap.Mode);
			var beatmapBytes = beatmap.FileBytes;
			if (beatmapBytes == null)
				return null;

			var beatmapTempFile = Path.GetTempFileName();
			File.WriteAllBytes(beatmapTempFile, beatmapBytes);

			var diffcalc = ruleset.CreateDifficultyCalculator(new FlatWorkingBeatmap(beatmapTempFile));
			return diffcalc.Calculate(mods);
		}
		public static DifficultyAttributes? CalculateDifficulty(APIMod[] mods, Beatmap beatmap)
		{
			var ruleset = GetRuleset((int)beatmap.Mode);
			var beatmapBytes = beatmap.FileBytes;
			if (beatmapBytes == null)
				return null;

			var beatmapTempFile = Path.GetTempFileName();
			File.WriteAllBytes(beatmapTempFile, beatmapBytes);

			var diffcalc = ruleset.CreateDifficultyCalculator(new FlatWorkingBeatmap(beatmapTempFile));
			return diffcalc.Calculate(mods.Select(x=> x.ToMod(ruleset)));
		}

		public static double? CalculatePerformance(double accuracy, Mod[] mods, DifficultyAttributes attributes, Beatmap beatmap)
		{
			var normalizedAccuracy = accuracy / 100.0;
			var score = new ScoreInfo
			{
				Accuracy = normalizedAccuracy,
				MaxCombo = beatmap.MaxCombo,
				APIMods = mods.Select(x=> new APIMod(x)).ToArray(),
				Statistics = GenerateHitResultsForRuleset(normalizedAccuracy, beatmap, 0),
				MaximumStatistics = new Dictionary<HitResult, int>
				{
					{HitResult.Great, beatmap.ObjectsTotal ?? 0}
				}
			};
			return CalculatePerformance(score, attributes, beatmap);
		}

		public static double? CalculatePerformance(double accuracy, Mod[] mods, Beatmap beatmap)
		{
			var attributes = CalculateDifficulty(mods, beatmap);
			if (attributes == null)
				return null;

			var normalizedAccuracy = accuracy / 100.0;

			var score = new ScoreInfo
			{
				Accuracy = normalizedAccuracy,
				MaxCombo = beatmap.MaxCombo,
				APIMods = mods.Select(x => new APIMod(x)).ToArray(),
				Statistics = GenerateHitResultsForRuleset(normalizedAccuracy, beatmap, 0),
				MaximumStatistics = new Dictionary<HitResult, int>
				{
					{HitResult.Great, beatmap.ObjectsTotal ?? 0}
				}
			};
			return CalculatePerformance(score, attributes, beatmap);
		}

		public static double? CalculatePerformance(ScoreInfo score, DifficultyAttributes attributes, Beatmap beatmap)
		{
			var ruleset = GetRuleset((int)beatmap.Mode);
			score.Ruleset = ruleset.RulesetInfo;

			var perfcalc = ruleset.CreatePerformanceCalculator();
			var ppAttributes = perfcalc?.Calculate(score, attributes);
			return ppAttributes?.Total;
		}

		public static double? CalculatePerformance(ScoreInfo score, Beatmap beatmap)
		{
			var ruleset = GetRuleset((int)beatmap.Mode);
			score.Ruleset = ruleset.RulesetInfo;

			var attributes = CalculateDifficulty(score.Mods, beatmap);
			if (attributes == null)
				return null;

			var perfcalc = ruleset.CreatePerformanceCalculator();
			var ppAttributes = perfcalc?.Calculate(score, attributes);
			return ppAttributes?.Total;
		}

		private static Ruleset GetRuleset(int id)
		{
			return id switch
			{
				0 => Osu,
				1 => Taiko,
				2 => Catch,
				3 => Mania,
				_ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
			};
		}

		public static Dictionary<HitResult, int> GenerateHitResultsForRuleset(double accuracy, Beatmap beatmap, int countMiss, int? countMeh = null, int? countGood = null, int? countLargeTickMisses = null, int? countSliderTailMisses = null)
		{
			var ruleset = GetRuleset((int)beatmap.Mode).RulesetInfo;

			return ruleset.OnlineID switch
			{
				0 => GenerateOsuHitResults(accuracy, beatmap, countMiss, countMeh, countGood, countLargeTickMisses, countSliderTailMisses),
				1 => GenerateTaikoHitResults(accuracy, beatmap, countMiss, countGood),
				2 => GenerateCatchHitResults(accuracy, beatmap, countMiss, countMeh, countGood),
				3 => GenerateManiaHitResults(accuracy, beatmap, countMiss),
				_ => throw new ArgumentException("Invalid ruleset ID provided.")
			};
		}

		private static Dictionary<HitResult, int> GenerateOsuHitResults(double accuracy, Beatmap beatmap, int countMiss, int? countMeh, int? countGood, int? countLargeTickMisses, int? countSliderTailMisses)
		{
			int countGreat;

			int totalResultCount = beatmap.ObjectsTotal ?? 0;

			if (countMeh != null || countGood != null)
			{
				countGreat = totalResultCount - (countGood ?? 0) - (countMeh ?? 0) - countMiss;
			}
			else
			{
				// Total result count excluding countMiss
				int relevantResultCount = totalResultCount - countMiss;

				// Accuracy excluding countMiss. We need that because we're trying to achieve target accuracy without touching countMiss
				// So it's better to pretened that there were 0 misses in the 1st place
				double relevantAccuracy = accuracy * totalResultCount / relevantResultCount;

				// Clamp accuracy to account for user trying to break the algorithm by inputting impossible values
				relevantAccuracy = Math.Clamp(relevantAccuracy, 0, 1);

				// Main curve for accuracy > 25%, the closer accuracy is to 25% - the more 50s it adds
				if (relevantAccuracy >= 0.25)
				{
					// Main curve. Zero 50s if accuracy is 100%, one 50 per 9 100s if accuracy is 75% (excluding misses), 4 50s per 9 100s if accuracy is 50%
					double ratio50To100 = Math.Pow(1 - (relevantAccuracy - 0.25) / 0.75, 2);

					// Derived from the formula: Accuracy = (6 * c300 + 2 * c100 + c50) / (6 * totalHits), assuming that c50 = c100 * ratio50to100
					double count100Estimate = 6 * relevantResultCount * (1 - relevantAccuracy) / (5 * ratio50To100 + 4);

					// Get count50 according to c50 = c100 * ratio50to100
					double count50Estimate = count100Estimate * ratio50To100;

					// Round it to get int number of 100s
					countGood = (int?)Math.Round(count100Estimate);

					// Get number of 50s as difference between total mistimed hits and count100
					countMeh = (int?)(Math.Round(count100Estimate + count50Estimate) - countGood);
				}
				// If accuracy is between 16.67% and 25% - we assume that we have no 300s
				else if (relevantAccuracy >= 1.0 / 6)
				{
					// Derived from the formula: Accuracy = (6 * c300 + 2 * c100 + c50) / (6 * totalHits), assuming that c300 = 0
					double count100Estimate = 6 * relevantResultCount * relevantAccuracy - relevantResultCount;

					// We only had 100s and 50s in that scenario so rest of the hits are 50s
					double count50Estimate = relevantResultCount - count100Estimate;

					// Round it to get int number of 100s
					countGood = (int?)Math.Round(count100Estimate);

					// Get number of 50s as difference between total mistimed hits and count100
					countMeh = (int?)(Math.Round(count100Estimate + count50Estimate) - countGood);
				}
				// If accuracy is less than 16.67% - it means that we have only 50s or misses
				// Assuming that we removed misses in the 1st place - that means that we need to add additional misses to achieve target accuracy
				else
				{
					// Derived from the formula: Accuracy = (6 * c300 + 2 * c100 + c50) / (6 * totalHits), assuming that c300 = c100 = 0
					double count50Estimate = 6 * relevantResultCount * relevantAccuracy;

					// We have 0 100s, because we can't start adding 100s again after reaching "only 50s" point
					countGood = 0;

					// Round it to get int number of 50s
					countMeh = (int?)Math.Round(count50Estimate);

					// Fill the rest results with misses overwriting initial countMiss
					countMiss = (int)(totalResultCount - countMeh);
				}

				// Rest of the hits are 300s
				countGreat = (int)(totalResultCount - countGood - countMeh - countMiss);
			}

			var result = new Dictionary<HitResult, int>
			{
				{ HitResult.Great, countGreat },
				{ HitResult.Ok, countGood ?? 0 },
				{ HitResult.Meh, countMeh ?? 0 },
				{ HitResult.Miss, countMiss }
			};

			if (countLargeTickMisses != null)
				result[HitResult.LargeTickMiss] = countLargeTickMisses.Value;

			if (countSliderTailMisses != null)
				result[HitResult.SliderTailHit] = beatmap.Sliders - countSliderTailMisses.Value;

			return result;
		}

		private static Dictionary<HitResult, int> GenerateTaikoHitResults(double accuracy, Beatmap beatmap, int countMiss, int? countGood)
		{
			int totalResultCount = beatmap.ObjectsTotal ?? 0;

			int countGreat;

			if (countGood != null)
			{
				countGreat = (int)(totalResultCount - countGood - countMiss);
			}
			else
			{
				// Let Great=2, Good=1, Miss=0. The total should be this.
				int targetTotal = (int)Math.Round(accuracy * totalResultCount * 2);

				countGreat = targetTotal - (totalResultCount - countMiss);
				countGood = totalResultCount - countGreat - countMiss;
			}

			return new Dictionary<HitResult, int>
			{
				{ HitResult.Great, countGreat },
				{ HitResult.Ok, (int)countGood },
				{ HitResult.Meh, 0 },
				{ HitResult.Miss, countMiss }
			};
		}

		private static Dictionary<HitResult, int> GenerateCatchHitResults(double accuracy, Beatmap beatmap, int countMiss, int? countMeh, int? countGood)
		{
			/*int maxCombo = beatmap.MaxCombo;

			int maxTinyDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<TinyDroplet>().Count());
			int maxDroplets = beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.NestedHitObjects.OfType<Droplet>().Count()) - maxTinyDroplets;
			int maxFruits = beatmap.HitObjects.OfType<Fruit>().Count() + 2 * beatmap.HitObjects.OfType<JuiceStream>().Count() + beatmap.HitObjects.OfType<JuiceStream>().Sum(s => s.RepeatCount);
			
			// Either given or max value minus misses
			int countDroplets = countGood ?? Math.Max(0, maxDroplets - countMiss);

			// Max value minus whatever misses are left. Negative if impossible missCount
			int countFruits = maxFruits - (countMiss - (maxDroplets - countDroplets));

			// Either given or the max amount of hit objects with respect to accuracy minus the already calculated fruits and drops.
			// Negative if accuracy not feasable with missCount.
			int countTinyDroplets = countMeh ?? (int)Math.Round(accuracy * (maxCombo + maxTinyDroplets)) - countFruits - countDroplets;

			// Whatever droplets are left
			int countTinyMisses = maxTinyDroplets - countTinyDroplets;
			
			return new Dictionary<HitResult, int>
			{
				{ HitResult.Great, countFruits },
				{ HitResult.LargeTickHit, countDroplets },
				{ HitResult.SmallTickHit, countTinyDroplets },
				{ HitResult.SmallTickMiss, countTinyMisses },
				{ HitResult.Miss, countMiss }
			};*/

			return new Dictionary<HitResult, int>
			{
				{ HitResult.Great, beatmap.ObjectsTotal ?? 0 },
				{ HitResult.Miss, countMiss }
			};
		}

		private static Dictionary<HitResult, int> GenerateManiaHitResults(double accuracy, Beatmap beatmap, int countMiss)
		{
			int totalResultCount = beatmap.ObjectsTotal ?? 0;

			// Let Great=6, Good=2, Meh=1, Miss=0. The total should be this.
			int targetTotal = (int)Math.Round(accuracy * totalResultCount * 6);

			// Start by assuming every non miss is a meh
			// This is how much increase is needed by greats and goods
			int delta = targetTotal - (totalResultCount - countMiss);

			// Each great increases total by 5 (great-meh=5)
			int countGreat = delta / 5;
			// Each good increases total by 1 (good-meh=1). Covers remaining difference.
			int countGood = delta % 5;
			// Mehs are left over. Could be negative if impossible value of amountMiss chosen
			int countMeh = totalResultCount - countGreat - countGood - countMiss;

			return new Dictionary<HitResult, int>
			{
				{ HitResult.Perfect, countGreat },
				{ HitResult.Great, 0 },
				{ HitResult.Good, countGood },
				{ HitResult.Ok, 0 },
				{ HitResult.Meh, countMeh },
				{ HitResult.Miss, countMiss }
			};
		}

		public static double GetAccuracyForRuleset(Beatmap beatmap, Dictionary<HitResult, int> statistics)
		{
			var ruleset = GetRuleset((int)beatmap.Mode).RulesetInfo;

			return ruleset.OnlineID switch
			{
				0 => GetOsuAccuracy(beatmap, statistics),
				1 => GetTaikoAccuracy(statistics),
				2 => GetCatchAccuracy(statistics),
				3 => GetManiaAccuracy(statistics),
				_ => 0.0
			};
		}

		private static double GetOsuAccuracy(Beatmap beatmap, Dictionary<HitResult, int> statistics)
		{
			int countGreat = statistics.GetValueOrDefault(HitResult.Great);
			int countGood = statistics.GetValueOrDefault(HitResult.Ok);
			int countMeh = statistics.GetValueOrDefault(HitResult.Meh);
			int countMiss = statistics.GetValueOrDefault(HitResult.Miss);

			double total = 6 * countGreat + 2 * countGood + countMeh;
			double max = 6 * (countGreat + countGood + countMeh + countMiss);

			if (statistics.TryGetValue(HitResult.SliderTailHit, out int countSliderTailHit))
			{
				int countSliders = beatmap.Sliders;

				total += 3 * countSliderTailHit;
				max += 3 * countSliders;
			}
			/*
			if (statistics.TryGetValue(HitResult.LargeTickMiss, out int countLargeTicksMiss))
			{
				int countLargeTicks = beatmap.HitObjects.Sum(obj => obj.NestedHitObjects.Count(x => x is SliderTick or SliderRepeat));
				int countLargeTickHit = countLargeTicks - countLargeTicksMiss;

				total += 0.6 * countLargeTickHit;
				max += 0.6 * countLargeTicks;
			}*/

			return total / max;
		}

		private static double GetTaikoAccuracy(Dictionary<HitResult, int> statistics)
		{
			int countGreat = statistics.GetValueOrDefault(HitResult.Great);
			int countGood = statistics.GetValueOrDefault(HitResult.Ok);
			int countMiss = statistics.GetValueOrDefault(HitResult.Miss);
			int total = countGreat + countGood + countMiss;

			return (double)((2 * countGreat) + countGood) / (2 * total);
		}

		private static double GetCatchAccuracy(Dictionary<HitResult, int> statistics)
		{
			double hits = statistics.GetValueOrDefault(HitResult.Great) + statistics.GetValueOrDefault(HitResult.LargeTickHit) + statistics.GetValueOrDefault(HitResult.SmallTickHit);
			double total = hits + statistics.GetValueOrDefault(HitResult.Miss) + statistics.GetValueOrDefault(HitResult.SmallTickMiss);

			return hits / total;
		}

		private static double GetManiaAccuracy(Dictionary<HitResult, int> statistics)
		{
			int countPerfect = statistics.GetValueOrDefault(HitResult.Perfect);
			int countGreat = statistics.GetValueOrDefault(HitResult.Great);
			int countGood = statistics.GetValueOrDefault(HitResult.Good);
			int countOk = statistics.GetValueOrDefault(HitResult.Ok);
			int countMeh = statistics.GetValueOrDefault(HitResult.Meh);
			int countMiss = statistics.GetValueOrDefault(HitResult.Miss);
			int total = countPerfect + countGreat + countGood + countOk + countMeh + countMiss;

			return (double)
				   ((6 * (countPerfect + countGreat)) + (4 * countGood) + (2 * countOk) + countMeh) /
				   (6 * total);
		}
	}
}
