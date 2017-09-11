// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using den0bot.DB;
using den0bot.Osu;

namespace den0bot.Modules
{
    class ModTopscores : IModule
    {
        private DateTime nextCheck;
        private Dictionary<int, List<Score>> storedTopscores;

        private int currentUser = 1;
        private Random rng = new Random();

        private readonly int scores_num = 5;
        private readonly double check_interval = 5; //seconds per player
        private readonly string api_id = Config.osu_token;

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender) => string.Empty;

        public ModTopscores()
        {
            nextCheck = DateTime.Now;
            storedTopscores = new Dictionary<int, List<Score>>();

#if !DEBUG
            Start();
#endif

            Log.Info(this, "Enabled");
        }

        private void Start()
        {
            Log.Info(this, "Loading...");

            for (int i = 1; i <= Database.GetPlayerCount(); i++)
            {
                uint id = Database.GetPlayerOsuID(i);
                if (id != 0)
                {
                    List<Score> topscores = Database.GetPlayerTopscores(i);
                    if (topscores != null)
                    {
                        storedTopscores.Add(i, topscores);
                        Log.Info(this, $"Restored {i} ({topscores[0].ScoreID.ToString()} {topscores[1].ScoreID.ToString()} {topscores[2].ScoreID.ToString()})");
                    }
                    else
                    {
                        topscores = OsuAPI.GetTopscores(id, scores_num);
                        if (topscores == null || topscores.Count <= 0)
                        {
                            Log.Error(this, $"Failed to add {i}!");
                            continue;
                        }
                        else
                        {
                            Database.SetPlayerTopscores(topscores, i);
                            storedTopscores.Add(i, topscores);
                            Log.Info(this, $"Added {i}");
                        }
                    }
                }
                else
                    Log.Info(this, $"Skipped {i}");
            }
        }


        public override void Think()
        {
#if !DEBUG
            if (nextCheck < DateTime.Now && storedTopscores.Count > 0)
            {
                Update();
                nextCheck = DateTime.Now.AddSeconds(check_interval);
            }
#endif
        }

        private void Update()
        {
            if (currentUser == Database.GetPlayerCount() + 1)
                currentUser = 1;

            uint userID = Database.GetPlayerOsuID(currentUser);
            if (userID == 0)
            {
                currentUser++;
                Update(); // we just do next user
                return;
            }

            List<Score> oldTopscores = storedTopscores[currentUser];
            List<Score> currentTopscores = OsuAPI.GetTopscores(userID, scores_num);
            if (currentTopscores == null || currentTopscores.Count <= 0)
                return;

            foreach(Score score in currentTopscores)
            {
                if (oldTopscores != null && 
                    !oldTopscores.Contains(score) && 
                    score.Pp > oldTopscores.Last().Pp &&
                    score.Date > DateTime.UtcNow.AddHours(-1))
                {
                    Map map = OsuAPI.GetBeatmap(score.BeatmapID);
                    Mods enabledMods = score.EnabledMods;

                    string mods = string.Empty;
                    if (enabledMods > 0)
                        mods = " +" + enabledMods.ToString().Replace(", ", "");

                    string mapInfo = string.Format("{0} - {1} [{2}]", map.Artist, map.Title, map.Difficulty).FilterToHTML();

                    string formattedMessage = string.Format("Там <b>{0}</b> фарманул новый скор: \n<i>{1}</i>{2} ({3}, {4}%) | <b>{5} пп</b>! Поздравим сраного фармера!",
                        Database.GetPlayerFriendlyName(currentUser), mapInfo, mods, score.Rank, score.Accuracy().ToString("N2"), score.Pp);

                    API.SendMessage(formattedMessage, Database.GetPlayerChatID(currentUser), Telegram.Bot.Types.Enums.ParseMode.Html);
                    break;
                }
            }

            Database.SetPlayerTopscores(currentTopscores, currentUser);
            storedTopscores[currentUser] = currentTopscores;

            currentUser++;
        }
    }
}
