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
        private Dictionary<int, List<Score>> storedTopscores;

        private int currentUser = 1;

#if !DEBUG
        private DateTime nextCheck;
        private readonly double check_interval = 5; //seconds per player
#endif

        private readonly int scores_num = 5;
        private readonly string api_id = Config.osu_token;

        public override string ProcessCommand(Telegram.Bot.Types.Message message) => string.Empty;

        public ModTopscores()
        {
            storedTopscores = new Dictionary<int, List<Score>>();

#if !DEBUG
            nextCheck = DateTime.Now;
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
                        AddPlayer(i);
                    }
                }
                else
                {
                    storedTopscores.Add(i, new List<Score>());
                    Log.Info(this, $"Skipped {i}");
                }
            }
        }

        public override void Think()
        {
#if !DEBUG
            if (nextCheck < DateTime.Now)
            {
                Update();
                nextCheck = DateTime.Now.AddSeconds(check_interval);
            }
#endif
        }

        private bool AddPlayer(int user)
        {
            uint id = Database.GetPlayerOsuID(user);
            if (id != 0)
            {
                List<Score> topscores = Database.GetPlayerTopscores(user);
                topscores = OsuAPI.GetTopscores(id, scores_num);
                if (topscores == null || topscores.Count <= 0)
                {
                    Log.Error(this, $"Failed to add {user}!");
                    return false;
                }
                else
                {
                    Database.SetPlayerTopscores(topscores, user);
                    storedTopscores.Add(user, topscores);
                    Log.Info(this, $"Added {user}");
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            int playerCount = Database.GetPlayerCount();
            if (playerCount <= 0)
                return;

            if (currentUser == playerCount + 1)
                currentUser = 1;

            uint userID = Database.GetPlayerOsuID(currentUser);
            if (userID == 0)
            {
                currentUser++;
                Update(); // we just do next user
                return;
            }

            if (storedTopscores.Count < currentUser)
            {
                if (!AddPlayer(currentUser))
                {
                    // skip
                    currentUser++;
                    Update();
                    return;
                }
            }
            List<Score> oldTopscores = storedTopscores[currentUser];
            List<Score> currentTopscores = OsuAPI.GetTopscores(userID, scores_num);
            if (currentTopscores == null || currentTopscores.Count <= 0)
            {
                currentUser++;
                return;
            }

            foreach(Score score in currentTopscores)
            {
                if (oldTopscores != null && 
                    !oldTopscores.Contains(score) && 
                    score.Pp > oldTopscores.Last().Pp &&
                    score.Date > DateTime.UtcNow.AddHours(7)) // osu is UTC+8
                {
                    Map map = OsuAPI.GetBeatmap(score.BeatmapID);
                    Mods enabledMods = score.EnabledMods;

                    string mods = string.Empty;
                    if (enabledMods > 0)
                        mods = " +" + enabledMods.ToString().Replace(", ", "");

                    string mapInfo = string.Format("{0} - {1} [{2}]", map.Artist, map.Title, map.Difficulty).FilterToHTML();

                    string formattedMessage = string.Format("Там <b>{0}</b> фарманул новый скор: \n<i>{1}</i>{2} ({3}, {4}%) | <b>{5} пп</b>! Поздравим сраного фармера!",
                        Database.GetPlayerFriendlyName(currentUser), mapInfo, mods, score.Rank, score.Accuracy.ToString("N2"), score.Pp);

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
