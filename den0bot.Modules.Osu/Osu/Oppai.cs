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
		public static double GetBeatmapPP(byte[] beatmap, Mods mods, double acc)
		{
			return GetBeatmapOppaiInfo(beatmap, mods, acc)?.PP ?? -1;
		}

		public static OppaiInfo GetBeatmapOppaiInfo(Map map, Score score = null)
		{
			if (score != null)
				return GetBeatmapOppaiInfo(map.FileBytes, score.EnabledMods ?? Mods.None, score.Accuracy, (int)score.Combo, (int)score.Misses);
			else
				return GetBeatmapOppaiInfo(map.FileBytes);
		}

		public static OppaiInfo GetBeatmapOppaiInfo(byte[] beatmap, Mods mods = Mods.None, double acc = -1, int combo = -1, int misses = 0)
		{
			try
			{
				using (var stream = new MemoryStream(beatmap, false))
				using (var reader = new StreamReader(stream, true))
				{
					Beatmap map = Beatmap.Read(reader);
					DiffCalc diff = new DiffCalc().Calc(map, (OppaiSharp.Mods) mods);

					PPv2 pp = new PPv2(new PPv2Parameters(map, diff,
						accuracy: acc / 100,
						cMiss: misses,
						combo: combo,
						mods: (OppaiSharp.Mods) mods)
					);

					return new OppaiInfo()
					{
						Stars = diff.Total,
						PP = pp.Total
					};
				}
			}
			catch (Exception e)
			{
				Log.Error($"GetBeatmapOppaiInfo failed, ${e.InnerMessageIfAny()}");
			}

			return null;
		}
	}
}
