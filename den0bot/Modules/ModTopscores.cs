// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;
using den0bot.Osu;

namespace den0bot.Modules
{
    class ModTopscores : IModule
    {
        private DateTime nextCheck;
        private Dictionary<int, List<Score>> latestTopscores;

        private int currentUser = 1;
        private Random rng = new Random();

        private readonly int scores_num = 5;
        private readonly double check_interval = 5; //seconds per player
        private readonly string api_id = Config.osu_token;

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender) => string.Empty;

        public ModTopscores()
        {
            nextCheck = DateTime.Now;
            latestTopscores = new Dictionary<int, List<Score>>();

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
                        latestTopscores.Add(i, topscores);
                        Log.Info(this, $"Restored {i} ({topscores[0].ScoreID.ToString()} {topscores[1].ScoreID.ToString()} {topscores[2].ScoreID.ToString()})");
                    }
                    else
                    {
                        topscores = OsuAPI.GetTopscores(id, scores_num);
                        if (topscores == null || topscores.Count <= 0)
                        {
                            List<Score> dummyscores = new List<Score>();
                            for (int j = 0; j < scores_num; j++)
                                dummyscores.Add(new Score() { Pp = 0 });

                            latestTopscores.Add(i, dummyscores);
                            continue;
                        }

                        Database.SetPlayerTopscores(topscores, i);
                        latestTopscores.Add(i, topscores);
                        Log.Info(this, $"Added {i}");
                    }
                }
            }
        }


        public override void Think()
        {
#if !DEBUG
            if (nextCheck < DateTime.Now && latestTopscores.Count > 0)
            {
                Update();
                nextCheck = DateTime.Now.AddSeconds(check_interval);
            }
#endif
        }

        private void Update()
        {
            currentUser++;

            if (currentUser == Database.GetPlayerCount() + 1)
                currentUser = 1;

            uint userID = Database.GetPlayerOsuID(currentUser);
            if (userID == 0)
            {
                Update(); // we just do next user
                return;
            }

            List<Score> oldTopscores = latestTopscores[currentUser];
            List<Score> currentTopscores = OsuAPI.GetTopscores(userID, scores_num);
            if (currentTopscores == null || currentTopscores.Count <= 0)
                return;

            for (int scoreNum = 0; scoreNum < currentTopscores.Count; scoreNum++)
            {
                if ((currentTopscores[scoreNum] != oldTopscores[scoreNum]) && (oldTopscores[scoreNum].Pp != 0))
                {
                    Map map = OsuAPI.GetBeatmap(currentTopscores[scoreNum].BeatmapID);
                    Mods enabledMods = currentTopscores[scoreNum].EnabledMods;

                    string mods = string.Empty;
                    if (enabledMods > 0)
                        mods = " +" + enabledMods.ToString().Replace(", ", "");

                    string mapInfo = string.Format("{0} - {1} [{2}]", map.Artist, map.Title, map.Difficulty).FilterToHTML();

                    string formattedMessage = string.Format("Там <b>{0}</b> фарманул новый скор: \n<i>{1}</i>{2} ({3}, {4}%) | <b>{5} пп</b>! Поздравим сраного фармера!",
                        Database.GetPlayerFriendlyName(currentUser), mapInfo, mods, currentTopscores[scoreNum].Rank, currentTopscores[scoreNum].Accuracy().ToString("N2"), currentTopscores[scoreNum].Pp);

                    API.SendMessage(formattedMessage, Database.GetPlayerChatID(currentUser), Telegram.Bot.Types.Enums.ParseMode.Html);
                    break;
                }
            }

            Database.SetPlayerTopscores(currentTopscores, currentUser);
            latestTopscores[currentUser] = currentTopscores;
        }
    }
}
