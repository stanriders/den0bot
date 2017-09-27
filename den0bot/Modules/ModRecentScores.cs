// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using den0bot.DB;
using den0bot.Osu;

namespace den0bot.Modules
{
    class ModRecentScores : IModule
    {
        public override string ProcessCommand(Telegram.Bot.Types.Message message)
        {
            if (message.Text.StartsWith("last"))
            {
                string playerID = string.Empty;
                int amount = 1;

                List<string> msgSplit = message.Text.Split(' ').ToList();
                if (msgSplit.Count >= 2)
                {
                    msgSplit.RemoveAt(0);

                    if (msgSplit.Count >= 2)
                    {
                        try
                        {
                            amount = int.Parse(msgSplit.Last());
                            if (amount > 10)
                                amount = 10;
                            msgSplit.Remove(msgSplit.Last());
                        }
                        catch { }
                    }
                    playerID = string.Join(" ", msgSplit);
                }
                else
                {
                    playerID = Database.GetPlayerOsuID(message.From.Username).ToString();
                }

                List<Score> lastScores = OsuAPI.GetRecentScores(playerID, amount);
                if (lastScores != null)
                {
                    string result = string.Empty;
                    foreach (Score score in lastScores)
                    {
                        Map map = OsuAPI.GetBeatmap(score.BeatmapID);

                        Mods enabledMods = score.EnabledMods;
                        string mods = string.Empty;
                        if (enabledMods > 0)
                            mods = " +" + enabledMods.ToString().Replace(", ", "");

                        string mapInfo = $"{map.Artist} - {map.Title} [{map.Difficulty}]";

                        TimeSpan ago = DateTime.Now.ToUniversalTime().AddHours(8) - score.Date; // osu is UTC+8
                        string date = ago.ToString(@"hh\:mm\:ss");

                        result += $"({score.Rank}) {mapInfo}{mods} ({score.Accuracy.ToString("N2")}%){Environment.NewLine}" +
                                  $"{score.Combo}/{map.MaxCombo}x ({score.Count300}/ {score.Count100} / {score.Count50} / {score.Misses}){Environment.NewLine}" +
                                  $"{date} ago{Environment.NewLine}{Environment.NewLine}";
                    }
                    return result;
                }
            }
            return string.Empty;
        }

        public override void Think(){}
    }
}
