// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using den0bot.Osu;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModBeatmap : IModule, IProcessAllMessages
    {
        private Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/([b,s]|(?>beatmapsets))\/(\d+\/?\#osu\/)?(\d+)\/?$|(?>https?:\/\/)?osu\.ppy\.sh\/([b,s]|(?>beatmapsets))\/(\d+\/?\#osu\/)?(\d+)\/?(?>[&,?].=\d)?\s?(\+.+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async void ReceiveMessage(Message message)
        {
            Match regexMatch = regex.Match(message.Text);
            if (regexMatch.Groups.Count > 1)
            {
                List<Group> listGroups = regexMatch.Groups.OfType<Group>().Where(x => (x != null) && (x.Length > 0)).ToList();

                bool isNew = listGroups[1].Value == "beatmapsets"; // are we using new website or not
                bool isSet = false;
                uint beatmapId = 0;
                string mods = string.Empty;

                if (isNew)
                {
                    if (listGroups[2].Value.Contains("#osu/"))
                    {
                        beatmapId = uint.Parse(listGroups[3].Value);
                        if (listGroups.Count > 4)
                            mods = listGroups[4].Value;
                    }
                    else
                    {
                        isSet = true;
                    }
                }
                else
                { 
                    if(listGroups[1].Value == "s")
                        isSet = true;

                    beatmapId = uint.Parse(listGroups[2].Value);
                    if (listGroups.Count > 3)
                        mods = listGroups[3].Value;
                }

                Map map = null;
                if (isSet)
                {
                    List<Map> set = await OsuAPI.GetBeatmapSetAsync(beatmapId);
                    if (set?.Count > 0)
                        map = set?.Last();
                }
                else
                {
                    map = await OsuAPI.GetBeatmapAsync(beatmapId);
                }

                API.SendPhoto(map?.Thumbnail, message.Chat, FormatMapInfo(map, mods), Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public static string FormatMapInfo(Map map, string mods)
        {
            if (map == null)
                return string.Empty;

            string result = string.Format("[{0}] - {1}* - {2}{3} - {4}\nCS: {5} | AR: {6} | OD: {7} | BPM: {8}",
                map.Difficulty, map.StarRating.FN2(), map.DrainLength(mods).ToString("mm':'ss"), $" - {map.Creator}", map.Status.ToString(),
                map.CS(mods).FN2(), map.AR(mods).FN2(), map.OD(mods).FN2(), map.BPM(mods).FN2());

            try
            {
                OppaiInfo info100 = Oppai.GetBeatmapOppaiInfo(map, 0, 100);
                if (info100 != null && info100.pp > 0)
                {
                    result += string.Format("\n100% - {0}pp", info100.pp.FN2());

                    if (Oppai.foundOppai) // temporary until i fix accuracy in oppai
                    {
                        OppaiInfo info98 = Oppai.GetBeatmapOppaiInfo(map, 0, 98);
                        OppaiInfo info95 = Oppai.GetBeatmapOppaiInfo(map, 0, 95);

                        if (info98 != null)
                            result += string.Format(" | 98% - {0}pp", info98.pp.FN2());
                        if (info95 != null)
                            result += string.Format(" | 95% - {0}pp", info95.pp.FN2());
                    }
                }
            }
            catch (Exception)
            { }

            result = result.FilterToHTML(); // remove any possible html stuff before adding our own

            result += $"\n[<a href=\"https://osu.ppy.sh/beatmapsets/{map.BeatmapSetID}/download\">Download</a>]";

            return result;
        }
    }
}
