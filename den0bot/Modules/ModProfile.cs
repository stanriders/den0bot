
using System;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModProfile : IModule
    {
        private readonly string api_id = Config.osu_token;

        public override bool NeedsAllMessages() => true;
        public override void Think() { }

        public override string ProcessCommand(string msg, Chat sender)
        {
            if (msg.Contains("osu.ppy.sh/u/") || msg.Contains("osu.ppy.sh/users/"))
            {
                string playerID = string.Empty;

                int index = msg.LastIndexOf("osu.ppy.sh/u/");
                if (index > 0)
                {
                    playerID = msg.Substring(index + 13);
                }
                else
                {
                    index = msg.LastIndexOf("osu.ppy.sh/users/");
                    if (index > 0)
                    {
                        playerID = msg.Substring(index + 17);
                    }
                }
                return FormatPlayerInfo(playerID);
            }
            return string.Empty;
        }

        private string FormatPlayerInfo(string playerID)
        {
            JToken info = OsuAPI.GetPlayerInfo(playerID);
            JArray topscores = OsuAPI.GetLastTopscores(int.Parse(info["user_id"].ToString()), 3);

            string username, rank, pp, playcount;
                username = info["username"].ToString();
                rank = info["pp_rank"].ToString();
                pp = info["pp_raw"].ToString();
                playcount = info["playcount"].ToString();

            string formatedTopscores = string.Empty;

            for (int i = 0; i < topscores.Count; i++)
            {
                JToken score = topscores[i];
                JToken map = OsuAPI.GetBeatmapInfo((uint)topscores[i]["beatmap_id"]);

                string mods = string.Empty;
                OsuAPI.Mods enabledMods = (OsuAPI.Mods)Enum.Parse(typeof(OsuAPI.Mods), score["enabled_mods"].ToString());
                if (enabledMods > 0)
                    mods = " +" + enabledMods.ToString().Replace(", ", "");

                // 1. Artist - Title [Diffname] +Mods - 123pp
                formatedTopscores += (i+1) + ". " + map["artist"].ToString() + " - " + map["title"].ToString() + " [" + map["version"].ToString() + "]" + mods + " - " + score["pp"] + "pp\n";
            }

            return username + " - #" + rank + " (" + pp + "pp)" + "\nPlaycount: " + playcount + "\n______\n" + formatedTopscores;
        }
    }
}
