// den0bot (c) StanR 2019 - MIT License
using System;
using System.IO;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Util;
using Mods = den0bot.Modules.Osu.Osu.Types.Mods;
using OppaiSharp;

namespace den0bot.Modules.Osu.Osu
{
	static class Oppai
	{
		public static double GetBeatmapPP(Map map, Mods mods, double acc)
		{
			return CalcPP(map.FileBytes, mods, acc)?.Total ?? -1;
		}

		public static double GetBeatmapPP(Map map, Score score)
		{
			return CalcPP(map.FileBytes, score.EnabledMods ?? Mods.None, score.Accuracy, (int)score.Combo, (int)score.Misses)?.Total ?? -1;
		}

		private static PPv2 CalcPP(byte[] beatmap, Mods mods = Mods.None, double acc = -1, int combo = -1, int misses = 0)
		{
			try
			{
				using (var stream = new MemoryStream(beatmap, false))
				using (var reader = new StreamReader(stream, true))
				{
					Beatmap map = Beatmap.Read(reader);
					DiffCalc diff = new DiffCalc().Calc(map, (OppaiSharp.Mods) mods);

					return new PPv2(new PPv2Parameters(map, diff, acc / 100, misses, combo, (OppaiSharp.Mods) mods));
				}
			}
			catch (Exception e)
			{
				Log.Error($"CalcPP failed, ${e.InnerMessageIfAny()}");
			}

			return null;
		}
	}
}
