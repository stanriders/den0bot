﻿// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using den0bot.Modules.Osu.Osu.API.Requests;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Util;

namespace den0bot.Modules.Osu
{
	public class ModMatchFollow : IModule, IReceiveAllMessages
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

		private readonly Regex matchLinkRegex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/community\/matches\/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex matchNameRegex = new Regex(@".+: ?\((.+)\) ?vs ?\((.+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
			Match regexMatch = matchLinkRegex.Match(msg.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => x.Length > 0).ToList();

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

			var match = await Osu.WebApi.MakeAPIRequest(new GetMatch(followList[currentMatch].MatchID));
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

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
			{
				Match regexMatch = matchLinkRegex.Match(message.Text);
				if (regexMatch.Groups.Count > 1)
				{
					List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => x.Length > 0).ToList();
					if (regexGroups.Count > 0 && ulong.TryParse(regexGroups[1].Value, out var matchID))
					{
						var match = await Osu.WebApi.MakeAPIRequest(new GetMatch(matchID));
						if (match?.Games.Count > 0)
							await API.SendMessage(await formatMatchInfo(match), message.Chat.Id,
								Telegram.Bot.Types.Enums.ParseMode.Html);
					}
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
				Map map = await Osu.WebApi.MakeAPIRequest(new GetBeatmap(game.BeatmapID));
				if (game.TeamMode >= MultiplayerMatch.TeamMode.Team)
				{
					int blueTotalScore = 0, redTotalScore = 0;
					string blueTeamName = "Blue team", redTeamName = "Red team";

					Match regexMatch = matchNameRegex.Match(match.Info.Name);
					if (regexMatch.Groups.Count == 3)
					{
						redTeamName = regexMatch.Groups[1].Value;
						blueTeamName = regexMatch.Groups[2].Value;
					}

					foreach (var g in match.Games)
					{
						var redGameScore = g.Scores.Sum(x => (x.Team == 2) ? x.ScorePoints : 0);
						var blueGameScore = g.Scores.Sum(x => (x.Team == 1) ? x.ScorePoints : 0);
						if (redGameScore > blueGameScore)
							redTotalScore++;
						else
							blueTotalScore++;
					}

					gamesString += $"{redTeamName} {redTotalScore} | {blueTeamName} {blueTotalScore}{Environment.NewLine}{Environment.NewLine}";

					List <Score> allScores = game.Scores;
					allScores = allScores.OrderByDescending(x => x.ScorePoints).ToList();
					allScores = allScores.OrderByDescending(x => x.Team).ToList();

					gamesString += $"<b>{map.Artist} - {map.Title} [{map.Difficulty}]</b>{Environment.NewLine}";
					foreach (var score in allScores)
					{
						if (score.ScorePoints != 0)
						{
							Player player = await Osu.WebApi.MakeAPIRequest(new GetUser(score.UserID.ToString()));
							string teamSymbol = score.Team > 1 ? "🔴" : "🔵";
							string pass = score.IsPass == 1 ? "" : ", failed";
							gamesString += $" {teamSymbol} <b>{player.Username}</b>: {score.ScorePoints} ({score.Combo}x, {score.Accuracy:N2}%{pass}){Environment.NewLine}";
						}
					}
					var redScore = allScores.Sum(x => (x.Team == 2) ? x.ScorePoints : 0);
					var blueScore = allScores.Sum(x => (x.Team == 1) ? x.ScorePoints : 0);
					if (redScore > blueScore)
						gamesString += $"{redTeamName} wins!";
					else
						gamesString += $"{blueTeamName} wins!";
				}
				else
				{
					game.Scores = game.Scores.OrderByDescending(x => x.ScorePoints).ToList();
					gamesString += $"{Environment.NewLine}{map.Artist} - {map.Title}[{map.Difficulty}]{Environment.NewLine}";
					for (int i = 0; i < game.Scores.Count(); i++)
					{
						if (game.Scores[i].ScorePoints != 0)
						{
							var player = await Osu.WebApi.MakeAPIRequest(new GetUser(game.Scores[i].UserID.ToString()));
							string pass = game.Scores[i].IsPass == 1 ? "" : ", failed";
							gamesString += $"{i + 1}. <b>{player.Username}</b>: {game.Scores[i].ScorePoints} ({game.Scores[i].Combo}x, {game.Scores[i].Accuracy:N2}%{pass}){Environment.NewLine}";
						}
					}
				}
			}
			return $"<a href=\"https://osu.ppy.sh/community/matches/{match.Info.ID}\">{match.Info.Name}</a>{Environment.NewLine}{gamesString}";
		}
	}
}
