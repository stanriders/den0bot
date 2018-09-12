// den0bot (c) StanR 2017 - MIT License
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using OppaiSharp;
namespace den0bot.Osu
{
    static class Oppai
    {
        public static bool foundOppai = true;

        public static void CheckOppai()
        {
            string oppaiPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\oppai.exe";
            if (File.Exists(oppaiPath))
            {
                Log.Info("Oppai", "Enabled");
            }
            else
            {
                foundOppai = false;
                Log.Error("Oppai", "oppai.exe not found! Using native implementation");
            }
        }
        public static OppaiInfo GetBeatmapOppaiInfo(Map map, Mods mods, double accuracy)
        {
            return GetBeatmapOppaiInfo(map, new Score() { EnabledMods = mods, Accuracy = accuracy, Combo = map.MaxCombo ?? 0, Count100 = 0, Count50 = 0, Misses = 0 });
        }
        public static OppaiInfo GetBeatmapOppaiInfo(Map map, Score score = null)
        {
            if (foundOppai)
            {
                string mods = string.Empty;

                if (score == null)
                    return GetBeatmapInfo_old(map.File, mods);

                Mods enabledMods = score.EnabledMods;
                if (enabledMods > 0)
                    mods = " +" + enabledMods.ToString().Replace(", ", "");

                return GetBeatmapInfo_old(map.File, mods, score.Accuracy, score.Combo, score.Misses);
            }
            else
            {
                if (score != null)
                    return GetBeatmapOppaiInfo_new(map.FileBytes, score.EnabledMods, score.Count100, score.Count50, score.Combo, score.Misses, score.Count300);
                else
                    return GetBeatmapOppaiInfo_new(map.FileBytes, 0);
            }
        }

        private static OppaiInfo GetBeatmapInfo_old(string beatmap, string mods, double accuracy = 100, uint combo = 0, uint misses = 0)
        {
            if (foundOppai)
            {
                string args = $"{mods} {accuracy}% ";
                if (combo != 0)
                    args += $"{combo}x ";
                if (misses != 0)
                    args += $"{misses}m";

                Process oppai = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "oppai.exe",
                        Arguments = $"- {args} -ojson",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true
                    }
                };
                oppai.Start();
                oppai.StandardInput.WriteLine(beatmap);
                oppai.StandardInput.Close();

                if (!oppai.StandardError.EndOfStream)
                {
                    Log.Error("Oppai", oppai.StandardError.ReadLine() + Environment.NewLine);
                    return null;
                }

                string data = string.Empty;
                while (!oppai.StandardOutput.EndOfStream)
                {
                    data += oppai.StandardOutput.ReadLine() + Environment.NewLine;
                }

                try
                {
                    return JsonConvert.DeserializeObject<OppaiInfo>(data);
                }
                catch (Exception e) { Log.Error("Oppai", e.InnerMessageIfAny()); }
            }
            return null;
        }
        private static OppaiInfo GetBeatmapOppaiInfo_new(byte[] beatmap, Mods mods, uint c100, uint c50, uint combo, uint misses, uint c300)
        {
            // using uints everywhere wasnt smart at all...
            return GetBeatmapOppaiInfo_new(beatmap, mods, (int) c100, (int) c50, combo == 0 ? -1 : (int) combo, (int) misses, c300 == 0 ? -1 : (int)c300);
        }
        private static OppaiInfo GetBeatmapOppaiInfo_new(byte[] beatmap, Mods mods, int c100 = 0, int c50 = 0, int combo = -1, int misses = 0, int c300 = -1)
        {
            try
            {
                var stream = new MemoryStream(beatmap, false);
                var reader = new StreamReader(stream, true);

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Beatmap map = Beatmap.Read(reader);
                DiffCalc diff = new DiffCalc().Calc(map, (OppaiSharp.Mods)mods);

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
            catch (Exception)
            {
                return new OppaiInfo()
                {
                    pp = -1
                };
            }
        }

        private static double GetBeatmapPP(byte[] beatmap, Mods mods, uint c300, uint c100, uint c50, uint combo, uint misses)
        {
            return GetBeatmapOppaiInfo_new(beatmap, mods, (int)c300, (int)c100, (int)c50, (int)combo, (int)misses).pp;
        }
        private static double GetBeatmapPP(byte[] beatmap, Mods mods, int c300 = -1, int c100 = 0, int c50 = 0, int combo = -1, int misses = 0)
        {
            return GetBeatmapOppaiInfo_new(beatmap, mods, c300, c100, c50, combo, misses).pp;
        }

    }
}
