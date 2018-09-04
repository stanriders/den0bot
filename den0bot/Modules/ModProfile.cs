// den0bot (c) StanR 2017 - MIT License
using System.Collections.Generic;
using Telegram.Bot.Types;
using den0bot.Osu;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace den0bot.Modules
{
    class ModProfile : IModule, IProcessAllMessages
    {
        private Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ModProfile() { Log.Info(this, "Enabled"); }

        public async void ReceiveMessage(Message message)
        {
            Match regexMatch = regex.Match(message.Text);
            if (regexMatch.Groups.Count > 1)
            {
                string playerID = regexMatch.Groups[1]?.Value;
                if (!string.IsNullOrEmpty(playerID))
                    API.SendMessage( await FormatPlayerInfo(playerID), message.Chat);
            }
        } 

        private async Task<string> FormatPlayerInfo(string playerID)
        {
            Player info = await OsuAPI.GetPlayerAsync(playerID);
            if (info == null)
                return string.Empty;

            List<Score> topscores = await OsuAPI.GetTopscoresAsync(info.ID, 3);
            if (topscores == null || topscores.Count <= 0)
                return string.Empty;

            string formatedTopscores = string.Empty;

            for (int i = 0; i < topscores.Count; i++)
            {
                Score score = topscores[i];
                Map map = await OsuAPI.GetBeatmapAsync(score.BeatmapID);

                string mods = string.Empty;
                Mods enabledMods = score.EnabledMods;
                if (enabledMods > 0)
                    mods = " +" + enabledMods.ToString().Replace(", ", "");

                // 1. Artist - Title [Diffname] +Mods (Rank, Accuracy%) - 123pp
                formatedTopscores += string.Format("{0}. {1} - {2} [{3}]{4} ({5}, {6}%) - {7}pp\n", (i+1), map.Artist, map.Title, map.Difficulty, mods, score.Rank, score.Accuracy.FN2(), score.Pp);
            }

            return string.Format("{0} - #{1} ({2}pp)\nPlaycount: {3}\n______\n{4}", info.Username, info.Rank, info.Pp, info.Playcount, formatedTopscores);
        }

    }
}
