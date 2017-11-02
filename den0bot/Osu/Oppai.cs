// den0bot (c) StanR 2017 - MIT License
using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace den0bot.Osu
{
    static class Oppai
    {
        public static OppaiInfo GetBeatmapInfo(string beatmap, string mods, double accuracy = 100, uint combo = 0, uint misses = 0)
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

            return null;
        }
    }
}
