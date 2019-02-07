// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Osu;
using den0bot.Osu.Types;
using den0bot.Util;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModRecentScores : IModule
	{
		private const int score_amount = 5;

		public ModRecentScores()
		{
			AddCommand(new Command
			{
				Name = "last",
				Reply = true,
				ActionAsync = GetScores,
				ParseMode = ParseMode.Html
			});
			Log.Debug(this, "Enabled");
		}

		private async Task<string> GetScores(Telegram.Bot.Types.Message message)
		{
			string playerID = string.Empty;
			int amount = 1;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0)
			{
				if (int.TryParse(msgSplit.Last(), out amount))
				{
					if (amount > score_amount)
						amount = score_amount;
				}
				msgSplit.Remove(msgSplit.Last());
			}

			if (msgSplit.Count > 0)
			{
				playerID = string.Join(" ", msgSplit);
			}
			else
			{
				playerID = Database.GetPlayerOsuID(message.From.Id).ToString();
				if (playerID == "0")
					return Localization.Get("recentscores_unknown_player", message.Chat.Id);
			}

			List<Score> lastScores = await OsuAPI.GetRecentScoresAsync(playerID, amount);
			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.Get("recentscores_no_scores", message.Chat.Id);

				string result = string.Empty;
				foreach (Score score in lastScores)
				{
					Mods enabledMods = score.EnabledMods ?? Mods.None;
					string mods = string.Empty;
					if (enabledMods > 0)
						mods = " +" + enabledMods.ToString().Replace(", ", "");

					TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date;
					string date = ago.ToString(@"hh\:mm\:ss") + " ago";

					Map map = await OsuAPI.GetBeatmapAsync(score.BeatmapID);
					if (map != null)
					{
						string mapInfo = $"{map.Artist} - {map.Title} [{map.Difficulty}]".FilterToHTML();

						result += $"<b>({score.Rank})</b> <a href=\"{map.Link}\">{mapInfo}</a><b>{mods} ({score.Accuracy.FN2()}%)</b>{Environment.NewLine}" +
								  $"{score.Combo}/{map.MaxCombo}x ({score.Count300}/ {score.Count100} / {score.Count50} / {score.Misses})";
						try
						{
							// Add pp values
							double scorePP = Oppai.GetBeatmapOppaiInfo(map, score).PP;
							string possiblePP = string.Empty;
							if (score.Combo < map.MaxCombo - 1 || score.Misses > 0 )
							{
								// Add possible pp value if they missed or dropped more than 1 combo
								Score fcScore = (Score)score.Clone();
								fcScore.Combo = map.MaxCombo ?? 0;
								fcScore.Misses = 0;
								double possiblePPval = Oppai.GetBeatmapOppaiInfo(map, fcScore).PP;
								possiblePP = $"({possiblePPval.FN2()}pp if FC)";
							}
							result += $" | ~{scorePP.FN2()}pp {possiblePP}";
						}
						catch(Exception) { }
					}
					else
					{
						// Didn't get beatmap info, insert plain link
						result += $"<b>({score.Rank})</b> https://osu.ppy.sh/b/{score.BeatmapID}<b>{mods} ({score.Accuracy.FN2()}%)</b>{Environment.NewLine}" +
								  $"{score.Combo}x ({score.Count300}/ {score.Count100} / {score.Count50} / {score.Misses})";
					}

					// Add date
					result += $"{Environment.NewLine}{date}{Environment.NewLine}{Environment.NewLine}";
				}

				return result;
			}
			return null;
		}
	}
}
