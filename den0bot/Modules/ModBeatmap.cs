using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using den0bot.Osu;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModBeatmap : IModule
    {
        public override bool NeedsAllMessages() => true;
        public override void Think() { }
        private bool foundOppai = true;
        public ModBeatmap()
        {
            string oppaiPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\oppai.exe";
            if (System.IO.File.Exists(oppaiPath))
                Log.Info(this, "Enabled");
            else
                Log.Error(this, "oppai.exe not found! ModBeatmap disabled.");
        }

        public override string ProcessCommand(string msg, Chat sender)
        {
            if (!foundOppai)
                return string.Empty;

            if (msg.Contains("osu.ppy.sh/b/") || msg.Contains("osu.ppy.sh/s/"))
            {
                Match regexMatch = Regex.Match(msg, @"https?:\/\/osu\.ppy\.sh\/([b,s])\/(\d+)$|https?:\/\/osu\.ppy\.sh\/([b,s])\/(\d+)[&,?].=\d\s?(\+.+)?|https?:\/\/osu\.ppy\.sh\/([b,s])\/(\d+)\s?(\+.+)");
                if (regexMatch.Groups.Count > 1)
                {
                    Group[] groups = new Group[9];
                    regexMatch.Groups.CopyTo(groups, 0);
                    List<Group> listGroups = groups.ToList();
                    listGroups.RemoveAll(x => (x.Length <= 0) || (x == null));

                    Map map = null;
                    string mods = string.Empty;
                    bool isSet = listGroups[1].Value == "s" ? true : false;

                    if (isSet)
                        map = OsuAPI.GetBeatmapSet(uint.Parse(listGroups[2].Value)).Last();
                    else
                        map = OsuAPI.GetBeatmap(uint.Parse(listGroups[2].Value));

                    if (listGroups.Count > 3)
                        mods = listGroups[3].Value;

                    return FormatMapInfo(map, mods);
                }
            }
            return string.Empty;
        }

        private string FormatMapInfo(Map map, string mods)
        {
            string result = string.Empty;
            string mapFile = string.Empty;

            if (map == null)
                return result;

            using (WebClient web = new WebClient())
            {
                mapFile = web.DownloadString("http://osu.ppy.sh/osu/" + map.BeatmapID);
                web.Dispose();
            }

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

            OppaiInfo info100 = Oppai.GetBeatmapInfo(mapFile, mods, 100);
            OppaiInfo info98 = Oppai.GetBeatmapInfo(mapFile, mods, 98);
            OppaiInfo info95 = Oppai.GetBeatmapInfo(mapFile, mods, 95);
            if (info100 != null)
            {
                result += string.Format("[{0}] - {1}* - {2}\nCS: {3} | AR: {4} | OD: {5} | BPM: {6}\n100% - {7}pp",
                    info100.version, info100.stars.ToString("N2"), drain.ToString("mm':'ss"),
                    info100.cs, info100.ar, info100.od, bpm.ToString("N2").Replace(" ",""), 
                    info100.pp.ToString("N2"));
            }
            if (info98 != null)
                result += string.Format(" | 98% - {0}pp", info98.pp.ToString("N2"));
            if (info95 != null)
                result += string.Format(" | 95% - {0}pp", info95.pp.ToString("N2"));

            return result;
        }

    }
}
