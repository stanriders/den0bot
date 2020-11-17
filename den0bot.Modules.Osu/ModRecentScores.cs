﻿// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Osu;
using den0bot.Modules.Osu.Osu.API.Requests.V1;
using den0bot.Modules.Osu.Osu.API.Requests.V2;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Modules.Osu.Osu.Types.V2;
using den0bot.Util;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules.Osu
{
	public class ModRecentScores : IModule
	{
		private readonly Regex profileRegex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private const int recent_amount = 5;
		private const int score_amount = 5;

		public ModRecentScores()
		{
			AddCommands(new[]
			{
				new Command
				{
					Name = "addme",
					ActionAsync = AddMe
				},
				new Command
				{
					Name = "removeme",
					Action = RemoveMe
				},
				new Command
				{
					Name = "removeplayer",
					IsOwnerOnly = true,
					Action = RemovePlayer
				},
				new Command
				{
					Names = {"last", "l"},
					Reply = true,
					ActionAsync = GetScores,
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Names = {"lastpass", "lp"},
					Reply = true,
					ActionAsync = GetPass,
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Names = {"score", "s"},
					Reply = true,
					ActionAsync = GetMapScores,
					ParseMode = ParseMode.Html
				}
			});
			Log.Debug("Enabled");
		}

		private async Task<string> GetScores(Telegram.Bot.Types.Message message)
		{
			string playerID;
			int amount = 1;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0)
			{
				if (int.TryParse(msgSplit.Last(), out amount))
				{
					if (amount > recent_amount)
						amount = recent_amount;

					msgSplit.Remove(msgSplit.Last());
				}
			}

			if (msgSplit.Count > 0)
			{
				playerID = string.Join(" ", msgSplit);
			}
			else
			{
				var id = DatabaseOsu.GetPlayerOsuIDFromDatabase(message.From.Id);
				if (id == 0)
					return Localization.Get("recentscores_unknown_player", message.Chat.Id);

				playerID = id.ToString();
			}

			List<Score> lastScores = await WebApi.MakeApiRequest(new GetUserScores(playerID, ScoreType.Recent, true));
			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.Get("recentscores_no_scores", message.Chat.Id);

				string result = string.Empty;
				foreach (var score in lastScores.Take(amount))
				{
					if (amount == 1)
						ChatBeatmapCache.StoreMap(message.Chat.Id, score.BeatmapShort.Id);

					score.Beatmap = await WebApi.MakeApiRequest(new Osu.API.Requests.V2.GetBeatmap(score.BeatmapShort.Id));
					result += FormatScore(score, true);
				}

				return result;
			}

			return Localization.Get("generic_fail", message.Chat.Id);
		}

		private async Task<string> GetPass(Telegram.Bot.Types.Message message)
		{
			string playerID;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0)
			{
				playerID = string.Join(" ", msgSplit);
			}
			else
			{
				var id = DatabaseOsu.GetPlayerOsuIDFromDatabase(message.From.Id);
				if (id == 0)
					return Localization.Get("recentscores_unknown_player", message.Chat.Id);

				playerID = id.ToString();
			}

			List<Score> lastScores = await WebApi.MakeApiRequest(new GetUserScores(playerID, ScoreType.Recent, false));
			if (lastScores.Count > 0)
			{
				var score = lastScores[0];
				ChatBeatmapCache.StoreMap(message.Chat.Id, score.BeatmapShort.Id);

				score.Beatmap = await WebApi.MakeApiRequest(new Osu.API.Requests.V2.GetBeatmap(score.BeatmapShort.Id));
				return FormatScore(score, true);
			}

			return Localization.Get("generic_fail", message.Chat.Id);
		}

		private async Task<string> GetMapScores(Telegram.Bot.Types.Message message)
		{
			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			uint mapId = 0;
			var mods = LegacyMods.None;
			if (msgSplit.Count > 0)
			{
				mapId = IBeatmap.GetIdFromLink(msgSplit.First(), out var isSet, out mods);

				if (isSet)
				{
					BeatmapSet set = await WebApi.MakeApiRequest(new Osu.API.Requests.V2.GetBeatmapSet(mapId));
					if (set?.Beatmaps?.Count > 0)
						mapId = set.Beatmaps.OrderBy(x => x.StarRating).Last().Id;
				}
			}
			else
			{
				mapId = ChatBeatmapCache.GetMap(message.Chat.Id);
				if (mapId == 0)
					return Localization.Get("generic_fail", message.Chat.Id);
			}

			var playerId = DatabaseOsu.GetPlayerOsuIDFromDatabase(message.From.Id);
			if (playerId == 0)
				return Localization.Get("recentscores_unknown_player", message.Chat.Id);

			List<Osu.Types.V1.Score> lastScores = await WebApi.MakeApiRequest(new GetScores(playerId.ToString(), mapId, mods, score_amount));

			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.Get("recentscores_no_scores", message.Chat.Id);

				ChatBeatmapCache.StoreMap(message.Chat.Id, mapId);

				string result = string.Empty;
				foreach (var score in lastScores)
				{
					score.Beatmap = await WebApi.MakeApiRequest(new Osu.API.Requests.V2.GetBeatmap(mapId));
					result += FormatScore(score, false);
				}

				return result;
			}

			return Localization.Get("generic_fail", message.Chat.Id);
		}

		private async Task<string> AddMe(Telegram.Bot.Types.Message message)
		{
			if (message.Text.Length > 6)
			{
				string player;
				Match regexMatch = profileRegex.Match(message.Text);
				if (regexMatch.Groups.Count > 1)
					player = regexMatch.Groups[1]?.Value;
				else
					player = message.Text.Substring(7);

				if (!string.IsNullOrEmpty(player))
				{
					if (!uint.TryParse(player, out var osuID))
					{
						// if they used /u/cookiezi instead of /u/124493 we ask osu API for an ID
						User info = await WebApi.MakeApiRequest(new Osu.API.Requests.V2.GetUser(player));

						if (info == null)
							return Localization.Get("recentscores_player_add_failed", message.Chat.Id);
						else
							osuID = info.Id;
					}

					if (osuID != 0)
					{
						DatabaseOsu.AddPlayerToDatabase(message.From.Id, osuID);
						return Localization.Get("recentscores_player_add_success", message.Chat.Id);
					}
				}
			}

			return Localization.Get("recentscores_player_add_failed", message.Chat.Id);
		}

		private string RemoveMe(Telegram.Bot.Types.Message message)
		{
			if (DatabaseOsu.RemovePlayerFromDatabase(message.From.Id))
				return Localization.Get("recentscores_player_remove_success", message.Chat.Id);
			else
				return Localization.Get("generic_fail", message.Chat.Id);
		}

		private string RemovePlayer(Telegram.Bot.Types.Message message)
		{
			string name = message.Text.Substring(14);

			if (!string.IsNullOrEmpty(name))
			{
				DatabaseOsu.RemovePlayerFromDatabase(Database.GetUserID(name));
				return Localization.Get("recentscores_player_remove_success", message.Chat.Id);
			}

			return Localization.Get("recentscores_player_remove_failed", message.Chat.Id);
		}

		private string FormatScore(IScore score, bool useAgo)
		{
			string mods = string.Empty;
			if (score.LegacyMods != LegacyMods.None)
				mods = $" +{score.LegacyMods.ToString().Replace(",", string.Empty).Replace(" ", string.Empty)}";

			string date = score.Date.ToShortDateString();
			if (useAgo)
			{
				TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date;
				date = $"{ago:hh\\:mm\\:ss} ago";
			}

			string result;

			Beatmap beatmap = (Beatmap)score.Beatmap;
			// html-filtered map title
			string mapInfo = $"{beatmap.BeatmapSet.Artist} - {beatmap.BeatmapSet.Title} [{score.Beatmap.Version}]".FilterToHTML();

			string pp = string.Empty;
			try
			{
				// Add pp values
				double? scorePP = score.Pp;
				if (scorePP == null)
					scorePP = Oppai.GetBeatmapPP(score.Beatmap, score);

				string possiblePP = string.Empty;

				if (score.Combo < beatmap.MaxCombo - 2 || score.Misses > 0)
				{
					// Add possible pp value if they missed or dropped more than 2 combo
					var fcScore = new Score
					{
						Statistics = new Score.ScoreStatistics
						{
							Count300 = (score.Beatmap.ObjectsTotal - score.Count100 - score.Count50) ?? 0,
							Count100 = score.Count100,
							Count50 = score.Count50,
						},
						Combo = beatmap.MaxCombo ?? 0,
						LegacyMods = score.LegacyMods
					};

					double possiblePPval = Oppai.GetBeatmapPP(score.Beatmap, fcScore);
					possiblePP = $"(~{possiblePPval:N2}pp if FC)";
				}

				pp = $"| {(scorePP == null ? "~" : "")}{scorePP:N2}pp {possiblePP}";
			}
			catch (Exception e)
			{
				Log.Error($"Oppai failed: {e.InnerMessageIfAny()}");
			}

			var completion = string.Empty;
			if (useAgo)
				completion = $" | {(double)(score.Count300 + score.Count100 + score.Count50 + score.Misses) / score.Beatmap.ObjectsTotal * 100.0:N1}% completion";

			result =
				$"<b>({score.Grade})</b> <a href=\"{score.Beatmap.Link}\">{mapInfo}</a><b>{mods} ({score.Accuracy:N2}%)</b>{Environment.NewLine}" +
				$"{score.Combo}/{beatmap.MaxCombo}x ({score.Count300} / {score.Count100} / {score.Count50} / {score.Misses}) {pp}{Environment.NewLine}" +
				$"{date}{completion}{Environment.NewLine}{Environment.NewLine}";

			return result;
		}


	}
}