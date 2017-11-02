// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using den0bot.Osu;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModBeatmap : IModule, IProcessAllMessages
    {
        private static bool foundOppai = true;
        private Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/([b,s])\/(\d+)$|(?>https?:\/\/)?osu\.ppy\.sh\/([b,s])\/(\d+)(?>[&,?].=\d)?\s?(\+.+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ModBeatmap()
        {
            string oppaiPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\oppai.exe";
            if (System.IO.File.Exists(oppaiPath))
            { 
                Log.Info(this, "Enabled");
            }
            else
            {
                foundOppai = false;
                Log.Error(this, "oppai.exe not found! ModBeatmap disabled.");
            }
        }

        public async void ReceiveMessage(Message message)
        {
            if (!foundOppai)
                return;

            Match regexMatch = regex.Match(message.Text);
            if (regexMatch.Groups.Count > 1)
            {
                List<Group> listGroups = regexMatch.Groups.OfType<Group>().Where(x => (x != null) && (x.Length > 0)).ToList();
                bool isSet = listGroups[1].Value == "s" ? true : false;

                Map map = null;
                if (isSet)
                {
                    List<Map> set = await OsuAPI.GetBeatmapSetAsync(uint.Parse(listGroups[2].Value));
                    map = set?.Last();
                }
                else
                {
                    map = await OsuAPI.GetBeatmapAsync(uint.Parse(listGroups[2].Value));
                }

                string mods = string.Empty;
                if (listGroups.Count > 3)
                    mods = listGroups[3].Value;

                API.SendPhoto(map?.Thumbnail, message.Chat, FormatMapInfo(map, mods));
            }
        }

        public static string FormatMapInfo(Map map, string mods)
        {
            string result = string.Empty;

            if (map == null)
                return result;

            string mapFile = map.File;

            TimeSpan drain = TimeSpan.FromSeconds(map.DrainLength);
            double bpm = map.BPM;
            if (mods.Contains("DT") || mods.Contains("NC"))
            {
                bpm *= 1.5;
                drain = TimeSpan.FromTicks((long)(drain.Ticks * 0.6666666));
            }
            else if (mods.Contains("HT"))
            {
                bpm *= 0.75;
                drain = TimeSpan.FromTicks((long)(drain.Ticks * 1.333333));
            }
            if (foundOppai)
            {
                OppaiInfo info100 = Oppai.GetBeatmapInfo(mapFile, mods, 100);
                OppaiInfo info98 = Oppai.GetBeatmapInfo(mapFile, mods, 98);
                OppaiInfo info95 = Oppai.GetBeatmapInfo(mapFile, mods, 95);
                if (info100 != null)
                {
                    result = string.Format("[{0}] - {1}* - {2} - {3}\nCS: {4} | AR: {5} | OD: {6} | BPM: {7}\n100% - {8}pp",
                        info100.version, info100.stars.ToString("N2"), drain.ToString("mm':'ss"), map.Status.ToString(),
                        map.CS, map.AR, map.OD, map.BPM,
                        info100.pp.ToString("N2"));
                }
                if (info98 != null)
                    result += string.Format(" | 98% - {0}pp", info98.pp.ToString("N2"));
                if (info95 != null)
                    result += string.Format(" | 95% - {0}pp", info95.pp.ToString("N2"));
            }
            else
            {
                result = string.Format("[{0}] - {1}* - {2} - {3}\nCS: {4} | AR: {5} | OD: {6} | BPM: {7}",
                        map.Difficulty, map.StarRating.ToString("N2"), drain.ToString("mm':'ss"), map.Status.ToString(),
                        map.CS, map.AR, map.OD, map.BPM);
            }
            return result;
        }

    /* "<b>[{0}]</b> - {1}* - {2}
     * <b>CS:</b> {3} | <b>AR:</b> {4} | <b>OD:</b> {5} | <b>BPM:</b> {6}
     * <b>100%</b> - {7}pp | <b>98%</b> - {0}pp | <b>95%</b> - {0}pp" */

    }
}
