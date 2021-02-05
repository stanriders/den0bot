// den0bot (c) StanR 2020 - MIT License
using System;
using System.IO;
using den0bot.Modules.Osu.Types;
using den0bot.Util;
using Newtonsoft.Json;
using OppaiSharp;

namespace den0bot.Modules.Osu
{
	public static class Oppai
	{
		private const string cache_path = "ppcache";

		static Oppai()
		{
			if (!Directory.Exists(cache_path))
				Directory.CreateDirectory(cache_path);
		}

		public static double GetBeatmapPP(IBeatmap map, LegacyMods mods, double acc)
		{
			var pp = CalcPP(map, mods, acc, cache: map.Ranked);
			return pp?.Total ?? -1;
		}

		public static double GetBeatmapPP(IBeatmap map, IScore score)
		{
			var pp = CalcPP(map, score.LegacyMods ?? LegacyMods.NM, score.Accuracy, (int)score.Combo, (int)score.Misses, map.Ranked);
			return pp?.Total ?? -1;
		}

		private static PPv2 CalcPP(IBeatmap beatmap, LegacyMods mods = LegacyMods.NM, double acc = -1, int combo = -1, int misses = 0, bool cache = false)
		{
			try
			{
				var mapCachePath = Path.Combine(cache_path, $"{beatmap.Id}_{(int)(mods & LegacyMods.DifficultyChanging)}.json");
				var ppParams = GetCachedPPv2Parameters(mapCachePath);
				if (ppParams != null)
				{
					ppParams.Accuracy = acc / 100;
					ppParams.Combo = combo;
					ppParams.CountMiss = misses;
					ppParams.Mods = (Mods)mods;
					return new PPv2(ppParams);
				}

				using (var stream = new MemoryStream(beatmap.FileBytes, false))
				using (var reader = new StreamReader(stream, true))
				{
					Beatmap map = Beatmap.Read(reader);
					DiffCalc diff = new DiffCalc().Calc(map, (Mods) mods);
					ppParams = new PPv2Parameters(map, diff, acc / 100, misses, combo, (Mods)mods);

					if (cache)
						CachePPv2Parameters(mapCachePath, ppParams);

					return new PPv2(ppParams);
				}
			}
			catch (Exception e)
			{
				Log.Error($"CalcPP failed, ${e.InnerMessageIfAny()}");
			}

			return null;
		}

		private static void CachePPv2Parameters(string mapCachePath, PPv2Parameters pp)
		{
			try
			{
				File.WriteAllText(mapCachePath, JsonConvert.SerializeObject(pp));
			}
			catch (Exception e)
			{
				Console.WriteLine($"Oppai: Failed to cache ppv2 params. {e}");
			}
		}

		private static PPv2Parameters GetCachedPPv2Parameters(string mapCachePath)
		{
			try
			{
				if (File.Exists(mapCachePath))
					return JsonConvert.DeserializeObject<PPv2Parameters>(File.ReadAllText(mapCachePath));
			}
			catch (Exception e)
			{
				Console.WriteLine($"Oppai: Failed to get cached ppv2 params. {e}");
			}
			return null;
		}
	}
}
