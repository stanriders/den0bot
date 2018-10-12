// den0bot (c) StanR 2018 - MIT License
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using OppaiSharp;

namespace den0bot.Osu
{
    static class Oppai
    {
		public static double GetBeatmapPP(byte[] beatmap, Mods mods, double acc)
		{
			return GetBeatmapOppaiInfo(beatmap, mods, acc)?.pp ?? -1;
		}

		public static OppaiInfo GetBeatmapOppaiInfo(Map map, Score score = null)
		{
			if (score != null)
				return GetBeatmapOppaiInfo(map.FileBytes, score.EnabledMods, -1, (int)score.Count300, (int)score.Count100, (int)score.Count50, (int)score.Combo, (int)score.Misses);
			else
				return GetBeatmapOppaiInfo(map.FileBytes);
		}

		public static OppaiInfo GetBeatmapOppaiInfo(byte[] beatmap, Mods mods = Mods.None, double acc = -1, int c300 = -1, int c100 = 0, int c50 = 0, int combo = -1, int misses = 0)
        {
            try
            {
                var stream = new MemoryStream(beatmap, false);
                var reader = new StreamReader(stream, true);

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Beatmap map = Beatmap.Read(reader);
                DiffCalc diff = new DiffCalc().Calc(map, (OppaiSharp.Mods)mods);

				if (acc != -1)
				{
					int objectCount = map.CountCircles + map.CountSliders + map.CountSpinners;
					AccuracyToHits(acc, objectCount, misses, out c300, out c100, out c50);
				}

                PPv2 pp = new PPv2(new PPv2Parameters(map, diff,
                    c300: c300,
                    c100: c100,
                    c50: c50,
                    cMiss: misses,
                    combo: combo,
                    mods: (OppaiSharp.Mods)mods)
                    );

                return new OppaiInfo()
                {
                    version = map.Version,
                    stars = diff.Total,
                    aim = pp.Aim,
                    speed = pp.Speed,
                    pp = pp.Total
                };
            }
            catch (Exception){ }

			return null;
		}

		private static void AccuracyToHits(double acc, int objCount, int misses, out int count300, out int count100, out int count50)
		{
			// straight outta oppai-ng
			double maxacc = 100; // TODO: actual maxacc!!!!!!!!!
			misses = Math.Min(objCount, misses);
			int max300 = objCount - misses;
			double accuracy = Math.Max(0.0, Math.Min(maxacc, acc));

			int c50 = 0;

			/* just some black magic maths from wolfram alpha */
			int c100 = (int)
				Math.Floor(-3.0 * ((accuracy * 0.01 - 1.0) *
					objCount + misses) * 0.5);

			if (c100 > objCount - misses)
			{
				/* acc lower than all 100s, use 50s */
				c100 = 0;
				c50 = (int)
					Math.Floor(-6.0 * ((accuracy * 0.01 - 1.0) *
						objCount + misses) * 0.2);

				c50 = Math.Min(max300, c50);
			}
			else
			{
				c100 = Math.Min(max300, c100);
			}

			count300 = objCount - c100 - c50 - misses;
			count100 = c100;
			count50 = c50;
		}
    }
}
