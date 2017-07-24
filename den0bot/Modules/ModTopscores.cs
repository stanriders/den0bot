using System;
using System.Collections.Generic;
using den0bot.Osu;

namespace den0bot.Modules
{
    class ModTopscores : IModule
    {
        private DateTime nextCheck;
        private Dictionary<int, List<Score>> latestTopscores;

        private int currentUser = 0;
        private Random rng = new Random();

        private readonly int scores_num = 5; 
        private readonly double check_interval = 0.1; //minutes
        private readonly string api_id = Config.osu_token;

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

            for (int i = 0; i < (int)Users.UserCount; i++)
            {
                uint id = Extensions.GetUserID((Users)i);
                if (id != 0)
                {
                    List<Score> topscores = OsuAPI.GetTopscores(id, scores_num);
                    if (topscores == null || topscores.Count <= 0)
                    {
                        latestTopscores.Add(i, new List<Score>() { new Score() { Pp = 0 } });
                        continue;
                    }

                    latestTopscores.Add(i, topscores);
                }
            }
        }

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            return string.Empty;
        }

        public override void Think()
        {
#if !DEBUG
            if (nextCheck < DateTime.Now)
            {
                Update();
                nextCheck = DateTime.Now.AddMinutes(check_interval);
            }
#endif
        }

        private void Update()
        {
            try
            {
                currentUser++;

                if (currentUser == (int)Users.UserCount)
                    currentUser = 0;

                uint userID = Extensions.GetUserID((Users)currentUser);
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
                    if (currentTopscores[scoreNum] != oldTopscores[scoreNum])
                    {
                        if (oldTopscores[scoreNum].Pp != 0)
                        {
                            Map map = OsuAPI.GetBeatmap(currentTopscores[scoreNum].BeatmapID);
                            string mapInfo = Extensions.FilterToHTML(map.Artist + " - " + map.Title + " [" + map.Difficulty + "]");

                            API.SendMessageToAllChats("Там <b>" + Extensions.GetUsername((Users)currentUser) + "</b> фарманул новый скор: \n<i>" + mapInfo + "</i> | <b>" + currentTopscores[scoreNum].Pp + " пп</b>! Поздравим сраного фармера!", null, Telegram.Bot.Types.Enums.ParseMode.Html);
                        }
                        break;
                    }
                }

                latestTopscores[currentUser] = currentTopscores;
            }
            catch (Exception ex) { Log.Error(this, "Update - " + ex.Message); }
        }
    }
}
