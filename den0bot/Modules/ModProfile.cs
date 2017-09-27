// den0bot (c) StanR 2017 - MIT License
using System.Collections.Generic;
using Telegram.Bot.Types;
using den0bot.Osu;
using System.Text.RegularExpressions;

namespace den0bot.Modules
{
    class ModProfile : IModule
    {
        private readonly string api_id = Config.osu_token;

        public override bool NeedsAllMessages => true;
        public override void Think() { }
        public ModProfile() { Log.Info(this, "Enabled"); }

        public override string ProcessCommand(Telegram.Bot.Types.Message message)
        {
            Match regexMatch = Regex.Match(message.Text, @"(?>https?:\/\/)?osu\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (regexMatch.Groups.Count > 1)
            {
                string playerID = regexMatch.Groups[1]?.Value;
                if (!string.IsNullOrEmpty(playerID))
                    return FormatPlayerInfo(playerID);
            }
            return string.Empty;
        }

        private string FormatPlayerInfo(string playerID)
        {
            Player info = OsuAPI.GetPlayer(playerID);
            if (info == null)
                return string.Empty;

            List<Score> topscores = OsuAPI.GetTopscores(info.ID, 3);
            if (topscores == null || topscores.Count <= 0)
                return string.Empty;

            string formatedTopscores = string.Empty;

            for (int i = 0; i < topscores.Count; i++)
            {
                Score score = topscores[i];
                Map map = OsuAPI.GetBeatmap(score.BeatmapID);

                string mods = string.Empty;
                Mods enabledMods = score.EnabledMods;
                if (enabledMods > 0)
                    mods = " +" + enabledMods.ToString().Replace(", ", "");

                // 1. Artist - Title [Diffname] +Mods - 123pp
                formatedTopscores += string.Format("{0}. {1} - {2} [{3}]{4} ({5}, {6}%) - {7}pp\n", (i+1), map.Artist, map.Title, map.Difficulty, mods, score.Rank, score.Accuracy.ToString("N2"), score.Pp);
            }

            return string.Format("{0} - #{1} ({2}pp)\nPlaycount: {3}\n______\n{4}", info.Username, info.Rank, info.Pp, info.Playcount, formatedTopscores);
        }
    }
}
