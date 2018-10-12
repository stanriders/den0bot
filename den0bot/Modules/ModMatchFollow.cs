// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using den0bot.Osu;

namespace den0bot.Modules
{
    class ModMatchFollow : IModule
    {
        private List<uint> followList = new List<uint>();
        private DateTime nextCheck = DateTime.Now;

		private Regex regex = new Regex(@"(?>https?:\/\/)?osu\.ppy\.sh\/community\/matches\/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public ModMatchFollow()
        {
            AddCommands(new Command[] 
            {
				new Command()
				{
					Name = "followmatch",
					Action = (msg) => StartFollowing(msg)
				},
				new Command()
				{
					Name = "matchinfo",
					ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html,
					ActionAsync = (msg) => MatchInfo(msg)
				}
            });
        }
        private string StartFollowing(Message msg)
        {
            //followList.Add()
            //return "Добавил!";
        }

        public override void Think()
        {
            if (followList.Count > 0 && nextCheck < DateTime.Now)
            {
                Update();
            }
        }

        private /*async*/ void Update()
        {

        }
		private async Task<string> MatchInfo(Message message)
		{
			Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => (x != null) && (x.Length > 0)).ToList();

				ulong matchID = ulong.Parse(regexGroups[1].Value);
				var match = await OsuAPI.GetMatch(matchID);

				string gamesString = string.Empty;
				List<MultiplayerMatch.Game> games = match.Games;
				var game = games.Last();
				var map = await OsuAPI.GetBeatmapAsync(game.BeatmapID);
				if (game.TeamMode >= MultiplayerMatch.TeamMode.Team)
				{
					List<Score> allScores = new List<Score>();

					allScores = game.Scores;
					allScores = allScores.OrderByDescending(x => x.ScorePoints).ToList();
					allScores = allScores.OrderByDescending(x => x.Team).ToList();

					gamesString += $"<b>{map.Artist} - {map.Title}[{map.Difficulty}]{Environment.NewLine}</b>";
					foreach (var score in allScores)
					{
						if (score.ScorePoints != 0)
						{
							var player = await OsuAPI.GetPlayerAsync(score.UserID.ToString());
							string teamSymbol = score.Team > 1 ? "🔴" : "🔵";
							gamesString += $" {teamSymbol} <b>{player.Username}</b>: {score.ScorePoints} ({score.Combo}x){Environment.NewLine}";
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
							var player = await OsuAPI.GetPlayerAsync(game.Scores[i].UserID.ToString());
							gamesString += $"{i+1}. <b>{player.Username}</b>: {game.Scores[i].ScorePoints} ({game.Scores[i].Combo}x){Environment.NewLine}";
						}
					}
					
				}
				
				return $"{match.Info.Name} - {match.Info.StartTime}{Environment.NewLine}{Environment.NewLine}{gamesString}";
			}
			return string.Empty;
		}
    }
}
