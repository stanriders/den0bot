// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using den0bot.DB;
using den0bot.Osu;
using den0bot.Util;

namespace den0bot.Modules
{
	class ModTopscores : IModule
	{
		private readonly Dictionary<int, List<Score>> storedTopscores;

		private int currentUser = 1;

		private DateTime nextCheck;
		private readonly double check_interval = 5; //seconds per player

		private readonly int scores_num = 5;

		public ModTopscores()
		{
			storedTopscores = new Dictionary<int, List<Score>>();

			nextCheck = DateTime.Now;
			Start();
			Log.Info(this, "Enabled");
		}

		private void Start()
		{
			Log.Debug(this, "Loading...");

			for (int i = 1; i <= Database.GetPlayerCount(); i++)
			{
				uint id = Database.GetPlayerOsuID(i);
				if (id != 0)
				{
					List<Score> topscores = Database.GetPlayerTopscores(i);
					if (topscores != null)
					{
						// we found topscores in db
						storedTopscores.Add(i, topscores);
						Log.Info(this, $"Restored {i} ({topscores[0].ScoreID.ToString()} {topscores[1].ScoreID.ToString()} {topscores[2].ScoreID.ToString()})");
					}
					else
					{
						// populate db with topscores and add user to storedTopscores
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
			if (nextCheck < DateTime.Now)
			{
				Update();
				nextCheck = DateTime.Now.AddSeconds(check_interval);
			}
		}

		private bool AddPlayer(int user)
		{
			uint id = Database.GetPlayerOsuID(user);
			if (id != 0)
			{
				List<Score> topscores = OsuAPI.GetTopscoresAsync(id, scores_num).Result;
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

		private async void Update()
		{
			int playerCount = Database.GetPlayerCount();
			if (playerCount <= 0)
				return;

			if (currentUser >= playerCount + 1)
				currentUser = 1;

			uint userID = Database.GetPlayerOsuID(currentUser);
			if (userID == 0)
			{
				currentUser++;
				Update(); // we just do next user
				return;
			}

			// someone added player to db, add them to storedTopscores
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
			List<Score> currentTopscores = await OsuAPI.GetTopscoresAsync(userID, scores_num);
			if (currentTopscores != null && currentTopscores.Count > 0)
			{
				bool needDBUpdate = false;
				foreach (Score score in currentTopscores)
				{
					if (oldTopscores != null &&
						oldTopscores.Count == scores_num && // if we somehow have less topscores than we should, ignore it
						!oldTopscores.Contains(score) &&
						score.Pp > oldTopscores.Last().Pp && // if PP is smaller than scores_num'th topscore we dont want it
						score.Date > DateTime.UtcNow.AddHours(7)) // osu is UTC+8, we use topscores that aren't older than 1 hour
					{
						Map map = await OsuAPI.GetBeatmapAsync(score.BeatmapID);
						Mods enabledMods = score.EnabledMods ?? Mods.None;

						string mods = string.Empty;
						if (enabledMods > 0)
							mods = " +" + enabledMods.ToString().Replace(", ", "");

						string mapInfo = string.Format("{0} - {1} [{2}]", map.Artist, map.Title, map.Difficulty).FilterToHTML();

						string formattedMessage = string.Format("Там <b>{0}</b> фарманул новый скор: \n<i>{1}</i>{2} ({3}, {4}%) | <b>{5} пп</b>! Поздравим сраного фармера!",
							""/*Database.GetPlayerFriendlyName(currentUser)*/, mapInfo, mods, score.Rank, score.Accuracy.FN2(), score.Pp);

						API.SendMessage(formattedMessage, 0/*Database.GetPlayerChatID(currentUser)*/, Telegram.Bot.Types.Enums.ParseMode.Html).NoAwait();

						needDBUpdate = true;
						break;
					}
				}

				if (needDBUpdate)
					Database.SetPlayerTopscores(currentTopscores, currentUser);

				storedTopscores[currentUser] = currentTopscores;
			}
			currentUser++;
		}
	}
}