using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
    class ModTopscores : IModule
    {
        private DateTime nextCheck;
        private Dictionary<int, List<string>> latestTopscores;

        private int currentUser = 0;
        private Random rng = new Random();

        private readonly int scores_num = 5; 
        private readonly double check_interval = 0.1; //minutes
        private readonly string api_id = Config.osu_token;

        public ModTopscores()
        {
            nextCheck = DateTime.Now;
            latestTopscores = new Dictionary<int, List<string>>();

            Start();

            Log.Info(this, "Enabled");
        }

        private void Start()
        {
            Log.Info(this, "Loading...");

            for (int i = 0; i < (int)Users.UserCount; i++)
            {
                int id = Extensions.GetUserID((Users)i);
                if (id != 0)
                {
                    JArray topscores = OsuAPI.GetLastTopscores(id);

                    List<string> scores = new List<string>();
                    for (int j = 0; j < scores_num; j++)
                    {
                        scores.Add(topscores[j]["pp"].ToString());
                    }
                    latestTopscores.Add(i, scores);

                }
            }
        }

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            return string.Empty;
        }

        public override void Think()
        {
            if (nextCheck < DateTime.Now)
            {
                Update();
                nextCheck = DateTime.Now.AddMinutes(check_interval);
            }
        }

        private void Update()
        {
            try
            {
                currentUser++;

                if (currentUser == (int)Users.UserCount)
                    currentUser = 0;

                int userID = Extensions.GetUserID((Users)currentUser);
                if (userID == 0)
                {
                    Update(); // we just do next user
                    return;
                }

                JArray topscores = OsuAPI.GetLastTopscores(userID);
                List<string> scores = latestTopscores[currentUser];

                for (int scoreNum = 0; scoreNum < scores_num; scoreNum++)
                {
                    string topscoreString = topscores[scoreNum]["pp"].ToString();

                    if (topscoreString != scores[scoreNum])
                    {
                        JToken map = OsuAPI.GetBeatmapInfo((uint)topscores[scoreNum]["beatmap_id"]);
                        string mapInfo = Extensions.FilterToHTML(map["artist"].ToString() + " - " + map["title"].ToString() + " [" + map["version"].ToString() + "]");
                        API.SendMessageToAllChats("Там <b>" + Extensions.GetUsername((Users)currentUser) + "</b> фарманул новый скор: \n<i>" + mapInfo + "</i> | <b>" + topscoreString + " пп</b>! Поздравим сраного фармера!", null, Telegram.Bot.Types.Enums.ParseMode.Html);
                        for (int i = scoreNum; i < scores_num; i++ )
                            scores[i] = topscores[i]["pp"].ToString();

                        break;
                    }
                }

                if (latestTopscores[currentUser] != scores)
                    latestTopscores[currentUser] = scores;
            }
            catch (Exception ex) { Log.Error(this, "Update - " + ex.Message); }
        }
    }
}
