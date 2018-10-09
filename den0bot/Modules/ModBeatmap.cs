// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using den0bot.Osu;
using den0bot.Util;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModBeatmap : IModule, IReceiveAllMessages
	{
        private Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/([b,s]|(?>beatmapsets))\/(\d+\/?\#osu\/?)?(\d+)?\/?$|(?>https?:\/\/)?osu\.ppy\.sh\/([b,s]|(?>beatmapsets))\/(\d+\/?\#osu\/)?(\d+)?\/?(?>[&,?].=\d)?\s?\+(.+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
				if (map != null)
					API.SendPhoto(map?.Thumbnail, message.Chat, FormatMapInfo(map, mods, message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public static string FormatMapInfo(Map map, string mods, long chatID)
        {
			double starRating = map.StarRating;
			string pp = string.Empty;

			try
            {
				Mods modsEnum = mods.ConvertToMods();

				OppaiInfo info100 = Oppai.GetBeatmapOppaiInfo(map.FileBytes, modsEnum, 100);
				if (info100 != null && info100.pp > 0)
				{
					pp = string.Format("\n100% - {0}pp", info100.pp.FN2());
					starRating = info100.stars;

					double info98 = Oppai.GetBeatmapPP(map.FileBytes, modsEnum, 98);
					if (info98 != -1)
						pp += string.Format(" | 98% - {0}pp", info98.FN2());

					double info95 = Oppai.GetBeatmapPP(map.FileBytes, modsEnum, 95);
					if (info95 != -1)
						pp += string.Format(" | 95% - {0}pp", info95.FN2());
					
				}
            }
            catch (Exception)
            { }

			string result = string.Format("[{0}] - {1}* - {2}{3} - {4}\nCS: {5} | AR: {6} | OD: {7} | BPM: {8}",
				map.Difficulty, starRating.FN2(), map.DrainLength(mods).ToString("mm':'ss"), $" - {map.Creator}", map.Status.ToString(),
				map.CS(mods).FN2(), map.AR(mods).FN2(), map.OD(mods).FN2(), map.BPM(mods).FN2());

			result = result.FilterToHTML(); // remove any possible html stuff before adding our own
			result += pp;
			result += $"\n[<a href=\"https://osu.ppy.sh/beatmapsets/{map.BeatmapSetID}/download\">{Localization.Get("beatmap_download", chatID)}</a>]";

            return result;
        }
    }
}
