// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.Enums;
using Telegram.Bot.Types;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Util;
using den0bot.Modules.Osu.WebAPI;
using den0bot.Types;

namespace den0bot.Modules.Osu
{
	public class ModMatchFollow : OsuModule, IReceiveAllMessages
	{
		private class FollowedMatch
		{
			public ulong MatchId { get; init; }
			public long ChatId { get; init; }
			public uint CurrentEventId { get; set; }
			public MatchTeamStatus Status { get; set; }
		}

		private class MatchTeamStatus
		{
			public uint RedScore { get; set; }
			public uint BlueScore { get; set; }
			public MatchTeamNames Teams { get; set; }
		}

		private readonly List<FollowedMatch> followList = new();
		private DateTime nextCheck = DateTime.Now;

		private int currentMatchId = 0;
		private bool updating = false;

		private readonly int update_time = 1; //seconds

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
			ulong? matchId = MatchLinkParser.Parse(msg.Text)?.Id;
			if (matchId != null)
			{
				var match = new FollowedMatch
				{
					MatchId = matchId.Value,
					ChatId = msg.Chat.Id,
					CurrentEventId = 0
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

			if (currentMatchId >= followList.Count)
				currentMatchId = 0;

			var updatingMatch = followList[currentMatchId];

			var match = await WebApiHandler.MakeApiRequest(new GetMatch(updatingMatch.MatchId));
			if (match.Events.Length > 0)
			{
				if (match.Info.EndTime == null)
				{
					// match still running
					var latestEvent = match.Events.Last(x=> x.Game != null);
					if (latestEvent.Id != updatingMatch.CurrentEventId && latestEvent.Game.EndTime != null)
					{
						// current event isnt the one we have stored and it ended already

						if (updatingMatch.Status == null && latestEvent.Game?.TeamMode >= TeamMode.Team)
							updatingMatch.Status = PopulateMatchTeamStatus(match);

						var matchInfo =
							$"<a href=\"https://osu.ppy.sh/community/matches/{match.Info.Id}\">{match.Info.Name}</a>{Environment.NewLine}";

						if (updatingMatch.CurrentEventId != 0)
						{
							switch (latestEvent.Game.TeamMode)
							{
								case TeamMode.HeadToHead:
								case TeamMode.Tag:
									matchInfo += FormatHeadToHeadGame(match);
									break;
								case TeamMode.Team:
								case TeamMode.TeamTag:
									matchInfo += FormatTeamGame(match, updatingMatch.Status);
									break;
								default:
									throw new ArgumentException();
							}
						}

						await API.SendMessage(matchInfo, updatingMatch.ChatId, Telegram.Bot.Types.Enums.ParseMode.Html);

						updatingMatch.CurrentEventId = latestEvent.Id;
					}
				}
				else
				{ 
					// match has ended
					followList.RemoveAt(currentMatchId);
				}
			}
			currentMatchId++;
		}

		public async Task ReceiveMessage(Message message)
		{
			var matchId = MatchLinkParser.Parse(message.Text)?.Id;
			if (matchId != null)
			{
				var match = await WebApiHandler.MakeApiRequest(new GetMatch(matchId.Value));
				if (match?.Events.Length > 0)
				{
					var matchInfo =
						$"<a href=\"https://osu.ppy.sh/community/matches/{match.Info.Id}\">{match.Info.Name}</a>{Environment.NewLine}";

					var latestEvent = match.Events.Last(x=> x.Game != null);
					switch (latestEvent.Game.TeamMode)
					{
						case TeamMode.HeadToHead:
						case TeamMode.Tag:
							matchInfo += FormatHeadToHeadGame(match);
							break;
						case TeamMode.Team:
						case TeamMode.TeamTag:
							matchInfo += FormatTeamGame(match, PopulateMatchTeamStatus(match));
							break;
						default:
							throw new ArgumentException();
					}

					await API.SendMessage(matchInfo, message.Chat.Id, Telegram.Bot.Types.Enums.ParseMode.Html);
				}
			}
		}

		private string FormatHeadToHeadGame(Match match)
		{
			string gamesString = string.Empty;
			var game = match.Events.Where(x => x.Game != null)
				.Select(x => x.Game)
				.Last(x => x.EndTime != null);

			var scores = game.Scores.OrderByDescending(x => x.Points).ToList();

			gamesString += $"{Environment.NewLine}{game.Beatmap.BeatmapSet.Artist} - {game.Beatmap.BeatmapSet.Title}[{game.Beatmap.Version}]{Environment.NewLine}";
			for (int i = 0; i < scores.Count; i++)
			{
				if (scores[i].Points != 0)
				{
					var player = match.Users.First(x => x.Id == scores[i].UserId);
					gamesString += $"{i + 1}. <b>{player.Username}</b>: {scores[i].Points} ({scores[i].Combo}x, {scores[i].Accuracy:N2}%{(scores[i].MatchData.Pass ? "" : ", failed")}){Environment.NewLine}";
				}
			}

			return gamesString;
		}

		private string FormatTeamGame(Match match, MatchTeamStatus status)
		{
			string gamesString = string.Empty;
			var game = match.Events.Where(x => x.Game != null)
				.Select(x => x.Game)
				.Last(x => x.EndTime != null);

			List<Score> allScores = game.Scores
				.OrderByDescending(x => x.Points)
				.ThenByDescending(x => x.MatchData.Team)
				.ToList();

			gamesString += $"{status.Teams.RedTeam} {status.RedScore} | {status.Teams.BlueTeam} {status.BlueScore}{Environment.NewLine}{Environment.NewLine}" +
			               $"<b>{game.Beatmap.BeatmapSet.Artist} - {game.Beatmap.BeatmapSet.Title} [{game.Beatmap.Version}]</b>{Environment.NewLine}";

			foreach (var score in allScores)
			{
				if (score.Points > 0)
				{
					var player = match.Users.First(x => x.Id == score.UserId);
					gamesString += $" {(score.MatchData.Team == Team.Red ? "🔴" : "🔵")} <b>{player.Username}</b>: {score.Points} ({score.Combo}x, {score.Accuracy:N2}%{(score.MatchData.Pass ? "" : ", failed")}){Environment.NewLine}";
				}
			}

			var redScore = allScores.Where(x => x.MatchData.Team == Team.Red).Sum(x => x.Points);
			var blueScore = allScores.Where(x => x.MatchData.Team == Team.Blue).Sum(x => x.Points);

			var redWon = redScore > blueScore;
			if (redWon)
				status.RedScore++;
			else
				status.BlueScore++;

			gamesString += $"<b>{(redWon ? status.Teams.RedTeam : status.Teams.BlueTeam)}</b> wins by <b>{Math.Abs(redScore - blueScore)}</b> points!";

			return gamesString;
		}

		private MatchTeamStatus PopulateMatchTeamStatus(Match match)
		{
			uint redTotalScore = 0, blueTotalScore = 0;
			foreach (var g in match.Events.Where(x=> x.Game != null).Select(x => x.Game))
			{
				var redGameScore = g.Scores.Where(x => x.MatchData.Team == Team.Red).Sum(x => x.Points);
				var blueGameScore = g.Scores.Where(x => x.MatchData.Team == Team.Blue).Sum(x => x.Points);

				if (redGameScore > blueGameScore)
					redTotalScore++;
				else
					blueTotalScore++;
			}

			return new MatchTeamStatus
			{
				RedScore = redTotalScore,
				BlueScore = blueTotalScore,
				Teams = MatchTeamsParser.Parse(match.Info.Name)
			};
		}

	}
}
