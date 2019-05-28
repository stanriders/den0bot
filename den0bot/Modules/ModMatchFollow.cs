// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using den0bot.Osu.API.Requests;
using den0bot.Osu.Types;
using den0bot.Util;

namespace den0bot.Modules
{
	class ModMatchFollow : IModule, IReceiveAllMessages
	{
		private class FollowedMatch
		{
			public ulong MatchID { get; set; }
			public long ChatID { get; set; }
			public uint CurrentGameID { get; set; }
		}

		private readonly List<FollowedMatch> followList = new List<FollowedMatch>();
		private DateTime nextCheck = DateTime.Now;

		private int currentMatch = 0;
		private bool updating = false;

		private readonly Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/community\/matches\/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly int update_time = 5; //seconds
	
		public ModMatchFollow()
		{
			AddCommands(new [] 
			{
				new Command()
				{
					Name = "followmatch",
					Action = StartFollowing
				}
			});
		}
		private string StartFollowing(Message msg)
		{
			Match regexMatch = regex.Match(msg.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => (x != null) && (x.Length > 0)).ToList();

				ulong matchID = ulong.Parse(regexGroups[1].Value);
				var match = new FollowedMatch()
				{
					MatchID = matchID,
					ChatID = msg.Chat.Id,
					CurrentGameID = 0
				};
				followList.Add(match);
				return Localization.Get("matchfollow_added", msg.Chat.Id);
			}
			return string.Empty;
		}

		public override void Think()
		{
			if (followList.Count > 0 && nextCheck < DateTime.Now && !updating)
			{
				Update();
			}
		}

		private async void Update()
		{
			updating = true; // thread racing :(

			if (currentMatch >= followList.Count)
				currentMatch = 0;

			var match = await Osu.WebApi.MakeAPIRequest(new GetMatch
			{
				ID = followList[currentMatch].MatchID
			});
			if (match.Games.Count > 0)
			{
				if (match.Info.EndTime != null)
					followList.RemoveAt(currentMatch);
				else
				{
					// match still running
					if (match.Games.Last().ID != followList[currentMatch].CurrentGameID &&
						match.Games.Last().EndTime != null)
					{
						// current game isnt the one we have stored and it ended already
						if (followList[currentMatch].CurrentGameID != 0)
							API.SendMessage(await formatMatchInfo(match), followList[currentMatch].ChatID, Telegram.Bot.Types.Enums.ParseMode.Html).NoAwait();

						followList[currentMatch].CurrentGameID = match.Games.Last().ID;
					}
				}
			}
			currentMatch++;
			nextCheck = DateTime.Now.AddSeconds(update_time);

			updating = false;
		}

		public async void ReceiveMessage(Message message)
		{
			Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => x.Length > 0).ToList();
				if (regexGroups.Count > 0 && ulong.TryParse(regexGroups[1].Value, out var matchID))
				{
					var match = await Osu.WebApi.MakeAPIRequest(new GetMatch
					{
						ID = matchID
					});

					if (match?.Games.Count > 0)
						API.SendMessage(await formatMatchInfo(match), message.Chat,
							Telegram.Bot.Types.Enums.ParseMode.Html);
				}
			}
		}

		private async Task<string> formatMatchInfo(MultiplayerMatch match)
		{
			string gamesString = string.Empty;

			List<MultiplayerMatch.Game> games = match.Games;
			var game = games.Last(x => x.EndTime != null);
			if (game?.Scores != null)
			{
				var map = await Osu.WebApi.MakeAPIRequest(new GetBeatmap
				{
					ID = game.BeatmapID

				});
				if (game.TeamMode >= MultiplayerMatch.TeamMode.Team)
				{
					List<Score> allScores = game.Scores;
					allScores = allScores.OrderByDescending(x => x.ScorePoints).ToList();
					allScores = allScores.OrderByDescending(x => x.Team).ToList();

					gamesString += $"<b>{map.Artist} - {map.Title}[{map.Difficulty}]{Environment.NewLine}</b>";
					foreach (var score in allScores)
					{
						if (score.ScorePoints != 0)
						{
							var player = await Osu.WebApi.MakeAPIRequest(new GetUser
							{
								Username = score.UserID.ToString()
							});
							string teamSymbol = score.Team > 1 ? "🔴" : "🔵";
							string pass = score.IsPass == 1 ? "" : ", failed";
							gamesString += $" {teamSymbol} <b>{player.Username}</b>: {score.ScorePoints} ({score.Combo}x, {score.Accuracy:N2}%{pass}){Environment.NewLine}";
						}
					}
					var redScore = allScores.Sum(x => (x.Team == 2) ? x.ScorePoints : 0);
					var blueScore = allScores.Sum(x => (x.Team == 1) ? x.ScorePoints : 0);
					if (redScore > blueScore)
						gamesString += "Red team wins!";
					else
						gamesString += "Blue team wins!";
				}
				else
				{
					game.Scores = game.Scores.OrderByDescending(x => x.ScorePoints).ToList();
					gamesString += $"{map.Artist} - {map.Title}[{map.Difficulty}]{Environment.NewLine}";
					for (int i = 0; i < game.Scores.Count(); i++)
					{
						if (game.Scores[i].ScorePoints != 0)
						{
							var player = await Osu.WebApi.MakeAPIRequest(new GetUser
							{
								Username = game.Scores[i].UserID.ToString()
							});
							string pass = game.Scores[i].IsPass == 1 ? "" : ", failed";
							gamesString += $"{i + 1}. <b>{player.Username}</b>: {game.Scores[i].ScorePoints} ({game.Scores[i].Combo}x, {game.Scores[i].Accuracy:N2}%{pass}){Environment.NewLine}";
						}
					}
				}
			}
			return $"<a href=\"https://osu.ppy.sh/community/matches/{match.Info.ID}\">{match.Info.Name}</a>{Environment.NewLine}{Environment.NewLine}{gamesString}";
		}
	}
}
