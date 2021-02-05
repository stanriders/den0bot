// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using den0bot.Modules.Osu.WebAPI.Requests.V1;
using den0bot.Modules.Osu.Types.V1;
using den0bot.Util;
using den0bot.Modules.Osu.WebAPI;
using den0bot.Types;

namespace den0bot.Modules.Osu
{
	public class ModMatchFollow : IModule, IReceiveAllMessages
	{
		private class FollowedMatch
		{
			public ulong MatchID { get; init; }
			public long ChatID { get; init; }
			public uint CurrentGameID { get; set; }
		}

		private readonly List<FollowedMatch> followList = new();
		private DateTime nextCheck = DateTime.Now;

		private int currentMatch = 0;
		private bool updating = false;

		private readonly Regex matchLinkRegex = new(@"(?>https?:\/\/)?osu\.ppy\.sh\/community\/matches\/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex matchNameRegex = new(@".+: ?\((.+)\) ?vs ?\((.+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly int update_time = 5; //seconds

		public ModMatchFollow()
		{
			AddCommands(new [] 
			{
				new Command
				{
					Name = "followmatch",
					Action = StartFollowing
				}
			});
		}
		private ICommandAnswer StartFollowing(Message msg)
		{
			Match regexMatch = matchLinkRegex.Match(msg.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => x.Length > 0).ToList();

				ulong matchID = ulong.Parse(regexGroups[1].Value);
				var match = new FollowedMatch
				{
					MatchID = matchID,
					ChatID = msg.Chat.Id,
					CurrentGameID = 0
				};
				followList.Add(match);
				return Localization.GetAnswer("matchfollow_added", msg.Chat.Id);
			}
			return null;
		}

		public override async void Think()
		{
			if (followList.Count > 0 && nextCheck < DateTime.Now && !updating)
			{
				updating = true;
				await Update();
				updating = false;
			}
		}

		private async Task Update()
		{
			nextCheck = DateTime.Now.AddSeconds(update_time);

			if (currentMatch >= followList.Count)
				currentMatch = 0;

			var match = await WebApiHandler.MakeApiRequest(new GetMatch(followList[currentMatch].MatchID));
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
							await API.SendMessage(await formatMatchInfo(match), followList[currentMatch].ChatID, Telegram.Bot.Types.Enums.ParseMode.Html);

						followList[currentMatch].CurrentGameID = match.Games.Last().ID;
					}
				}
			}
			currentMatch++;
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
						var match = await WebApiHandler.MakeApiRequest(new GetMatch(matchID));
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
				Map map = await WebApiHandler.MakeApiRequest(new GetBeatmap(game.BeatmapID));
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
						var redGameScore = g.Scores.Where(x => x.Team == Team.Red).Sum(x => x.ScorePoints);
						var blueGameScore = g.Scores.Where(x => x.Team == Team.Blue).Sum(x => x.ScorePoints);

						if (redGameScore > blueGameScore)
							redTotalScore++;
						else
							blueTotalScore++;
					}

					List <Score> allScores = game.Scores
						.OrderByDescending(x => x.ScorePoints)
						.ThenByDescending(x => x.Team)
						.ToList();

					gamesString += $"{redTeamName} {redTotalScore} | {blueTeamName} {blueTotalScore}{Environment.NewLine}{Environment.NewLine}" +
					               $"<b>{map.Artist} - {map.Title} [{map.Version}]</b>{Environment.NewLine}";

					foreach (var score in allScores)
					{
						if (score.ScorePoints > 0)
						{
							Player player = await WebApiHandler.MakeApiRequest(new GetUser(score.UserID.ToString()));
							gamesString += $" {(score.Team == Team.Red ? "🔴" : "🔵")} <b>{player.Username}</b>: {score.ScorePoints} ({score.Combo}x, {score.Accuracy:N2}%{(score.IsPass ? "" : ", failed")}){Environment.NewLine}";
						}
					}

					var redScore = allScores.Where(x => x.Team == Team.Red).Sum(x => x.ScorePoints);
					var blueScore = allScores.Where(x => x.Team == Team.Blue).Sum(x => x.ScorePoints);

					gamesString += $"<b>{(redScore > blueScore ? redTeamName : blueTeamName)}</b> wins by <b>{Math.Abs(redScore - blueScore)}</b> points!";
				}
				else if(game.TeamMode >= MultiplayerMatch.TeamMode.HeadToHead)
				{
					var scores = game.Scores.OrderByDescending(x => x.ScorePoints).ToList();

					gamesString += $"{Environment.NewLine}{map.Artist} - {map.Title}[{map.Version}]{Environment.NewLine}";
					for (int i = 0; i < scores.Count; i++)
					{
						if (scores[i].ScorePoints != 0)
						{
							Player player = await WebApiHandler.MakeApiRequest(new GetUser(scores[i].UserID.ToString()));
							gamesString += $"{i + 1}. <b>{player.Username}</b>: {scores[i].ScorePoints} ({scores[i].Combo}x, {scores[i].Accuracy:N2}%{(scores[i].IsPass ? "" : ", failed")}){Environment.NewLine}";
						}
					}
				}
			}
			return $"<a href=\"https://osu.ppy.sh/community/matches/{match.Info.ID}\">{match.Info.Name}</a>{Environment.NewLine}{gamesString}";
		}
	}
}
