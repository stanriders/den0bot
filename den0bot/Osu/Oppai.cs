using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace den0bot.Osu
{
    static class Oppai
    {
        public static OppaiInfo GetBeatmapInfo(string beatmap, string mods, double accuracy = 100)
        {
            Process oppai = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "oppai.exe",
                    Arguments = string.Format("- {0} {1}% -ojson", mods, accuracy),
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

            JToken info = JToken.Parse(data);
            if (info.Count() > 0)
            {
                OppaiInfo result = new OppaiInfo();
                result.version = info["version"].ToString();

                result.od = info["od"].Value<double>();
                result.ar = info["ar"].Value<double>();
                result.cs = info["cs"].Value<double>();
                result.hp = info["hp"].Value<double>();

                result.max_combo = info["max_combo"].Value<short>();

                result.num_circles = info["num_circles"].Value<short>();
                result.num_sliders = info["num_sliders"].Value<short>();
                result.num_spinners = info["num_spinners"].Value<short>();

                result.stars = info["stars"].Value<double>();
                result.speed = info["speed_stars"].Value<double>();
                result.aim = info["aim_stars"].Value<double>();

                result.pp = info["pp"].Value<double>();

                return result;
            }

            return null;
        }
    }
}
