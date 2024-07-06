// den0bot (c) StanR 2024 - MIT License
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Util;
using Telegram.Bot.Types.Enums;
using den0bot.Types;
using den0bot.Types.Answers;
using Score = den0bot.Modules.Osu.Types.V2.Score;
using Microsoft.Extensions.Logging;

namespace den0bot.Modules.Osu
{
	public class ModRecentScores : OsuModule
	{
		private readonly ILogger<IModule> logger;
		private const int recent_amount = 5;
		private const int score_amount = 5;

		public ModRecentScores(ILogger<IModule> logger) : base(logger)
		{
			this.logger = logger;
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
					ActionAsync = RemoveMe
				},
				new Command
				{
					Name = "removeplayer",
					IsOwnerOnly = true,
					ActionAsync = RemovePlayer
				},
				new Command
				{
					Names = {"last", "l"},
					Reply = true,
					Slow = true,
					ActionAsync = GetLastScores,
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Names = {"lastpass", "lp"},
					Reply = true,
					Slow = true,
					ActionAsync = GetPass,
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Names = {"score", "s"},
					Reply = true,
					Slow = true,
					ActionAsync = GetMapScores,
					ParseMode = ParseMode.Html
				}
			});
		}

		private async Task<ICommandAnswer> GetLastScores(Telegram.Bot.Types.Message message)
		{
			uint playerId = 0;
			int amount = 1;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0 && int.TryParse(msgSplit.Last(), out amount))
			{
				msgSplit.Remove(msgSplit.Last());
			}
			amount = Math.Clamp(amount, 1, recent_amount);

			if (msgSplit.Count > 0)
			{
				var playerName = string.Join(" ", msgSplit);
				var player = await new GetUser(playerName).Execute();
				if (player != null)
					playerId = player.Id;
			}
			else
			{
				await using var db = new DatabaseOsu();
				var id = db.Players.FirstOrDefault(x=> x.TelegramID == message.From.Id)?.OsuID;
				if (id == null || id == 0)
					return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

				playerId = id.Value;
			}

			if (playerId == 0)
				return Localization.GetAnswer("generic_fail", message.Chat.Id);

			List<LazerScore> lastScores = await new GetUserScores(playerId, ScoreType.Recent, true).Execute();
			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				string result = string.Empty;
				foreach (var score in lastScores.Take(amount))
				{
					if (amount == 1)
						ChatBeatmapCache.StoreLastMap(message.Chat.Id, new ChatBeatmapCache.CachedBeatmap { BeatmapId = score.BeatmapShort.Id, BeatmapSetId = score.BeatmapShort.BeatmapSetId });

					var beatmap = await new GetBeatmap(score.BeatmapShort.Id).Execute();
					result += await FormatLazerScore(score, beatmap, true);
				}

				return new TextCommandAnswer(result);
			}

			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> GetPass(Telegram.Bot.Types.Message message)
		{
			uint playerId = 0;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0)
			{
				var playerName = string.Join(" ", msgSplit);
				var player = await new GetUser(playerName).Execute();
				if (player != null)
					playerId = player.Id;
			}
			else
			{
				await using var db = new DatabaseOsu();
				var id = db.Players.FirstOrDefault(x => x.TelegramID == message.From.Id)?.OsuID;
				if (id == null || id == 0)
					return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

				playerId = id.Value;
			}

			if (playerId == 0)
				return Localization.GetAnswer("generic_fail", message.Chat.Id);

			var lastScores = await new GetUserScores(playerId, ScoreType.Recent, false).Execute();
			if (lastScores.Count > 0)
			{
				var score = lastScores[0];
				ChatBeatmapCache.StoreLastMap(message.Chat.Id, new ChatBeatmapCache.CachedBeatmap { BeatmapId = score.BeatmapShort.Id, BeatmapSetId = score.BeatmapShort.BeatmapSetId});

				var beatmap = await new GetBeatmap(score.BeatmapShort.Id).Execute();
				return new TextCommandAnswer(await FormatLazerScore(score, beatmap, true));
			}

			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> GetMapScores(Telegram.Bot.Types.Message message)
		{
			uint mapId = 0;

			// beatmap id regex can parse link as part of a complex message so we dont need to clean it up beforehand
			var msgText = message.Text;
			if (message.ReplyToMessage != null)
			{
				var sentMap = ChatBeatmapCache.GetSentMap(message.ReplyToMessage.MessageId);
				if (sentMap?.BeatmapId is not null)
					mapId = sentMap.BeatmapId;
				else if (message.ReplyToMessage.Text.Contains(".ppy.sh"))
					msgText = message.ReplyToMessage.Text;
			}

			var msgSplit = msgText.Split(' ').ToList();
			var mods = Array.Empty<Mod>();
			if (msgSplit.Count > 1)
			{
				var data = BeatmapLinkParser.Parse(msgText);
				if (data != null)
				{
					mapId = data.ID;
					mods = data.Mods;
					if (data.IsBeatmapset)
					{
						BeatmapSet set = await new GetBeatmapSet(data.ID).Execute();
						if (set?.Beatmaps?.Count > 0)
							mapId = set.Beatmaps.OrderBy(x => x.StarRating).Last().Id;
					}
				}
			}
			else if (mapId == 0)
			{
				mapId = ChatBeatmapCache.GetLastMap(message.Chat.Id)?.BeatmapId ?? 0;
			}

			if (mapId == 0)
				return Localization.GetAnswer("generic_fail", message.Chat.Id);

			await using var db = new DatabaseOsu();

			var playerId = db.Players.FirstOrDefault(x => x.TelegramID == message.From.Id)?.OsuID;
			if (playerId == null || playerId == 0)
				return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

			var result = string.Empty;

			if (mods.Length == 0)
			{
				var scores = await new GetUserBeatmapScores(mapId, playerId.Value).Execute();
				if (scores == null || scores.Count <= 0)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				var map = await new GetBeatmap(mapId).Execute();

				foreach (var score in scores.Take(score_amount))
				{
					result += await FormatScore(score, map, false);
				}
			}
			else
			{
				var score = await new GetUserBeatmapScore(mapId, playerId.Value, mods.Select(x=> x.Acronym).ToArray()).Execute();
				if (score == null)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				var beatmap = await new GetBeatmap(mapId).Execute();
				result += await FormatScore(score, beatmap, false);
			}
				
			if (!string.IsNullOrEmpty(result))
			{
				ChatBeatmapCache.StoreLastMap(message.Chat.Id, new ChatBeatmapCache.CachedBeatmap {BeatmapId = mapId});
				return new TextCommandAnswer(result);
			}
			
			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> AddMe(Telegram.Bot.Types.Message message)
		{
			if (message.Text.Length > 6)
			{
				await using var db = new DatabaseOsu();

				string player = ProfileLinkParser.Parse(message.Text)?.Id;
				if (string.IsNullOrEmpty(player))
					player = message.Text.Substring(7);

				if (!string.IsNullOrEmpty(player))
				{
					if (db.Players.Any(x => x.TelegramID == message.From.Id))
						return Localization.GetAnswer($"annoy_{RNG.NextNoMemory(1, 10)}", message.Chat.Id);

					if (!uint.TryParse(player, out var osuID))
					{
						// if they used /u/cookiezi instead of /u/124493 we ask osu API for an ID
						var info = await new GetUser(player).Execute();

						if (info == null)
							return Localization.GetAnswer("recentscores_player_add_failed", message.Chat.Id);
						else
							osuID = info.Id;
					}

					if (osuID != 0)
					{
						await db.Players.AddAsync(new DatabaseOsu.Player
						{
							OsuID = osuID,
							TelegramID = message.From.Id
						});
						await db.SaveChangesAsync();

						return Localization.GetAnswer("recentscores_player_add_success", message.Chat.Id);
					}
				}
			}

			return Localization.GetAnswer("recentscores_player_add_failed", message.Chat.Id);
		}

		private async Task<ICommandAnswer> RemoveMe(Telegram.Bot.Types.Message message)
		{
			await using (var db = new DatabaseOsu())
			{
				var player = db.Players.FirstOrDefault(x=> x.TelegramID == message.From.Id);
				if (player != null)
				{
					db.Players.Remove(player);
					await db.SaveChangesAsync();

					return Localization.GetAnswer("recentscores_player_remove_success", message.Chat.Id);
				}
				
				return Localization.GetAnswer("generic_fail", message.Chat.Id);
			}
		}

		private async Task<ICommandAnswer> RemovePlayer(Telegram.Bot.Types.Message message)
		{
			await using (var db = new DatabaseOsu())
			{
				var tgId = DatabaseCache.GetUserID(message.Text.Split()[1]);
				if (tgId != 0)
				{
					var player = db.Players.FirstOrDefault(x => x.TelegramID == tgId);
					if (player != null)
					{
						db.Players.Remove(player);
						await db.SaveChangesAsync();

						return Localization.GetAnswer("recentscores_player_remove_success", message.Chat.Id);
					}
				}

				return Localization.GetAnswer("generic_fail", message.Chat.Id);
			}
		}

		private async Task<string> FormatScore(Score score, Beatmap beatmap, bool useAgo)
		{
			string mods = string.Empty;
			if (score.Mods.Length > 0)
				mods = $" +{string.Join(null, score.Mods)}";

			string date = score.Date?.ToShortDateString();
			if (useAgo && score.Date != null)
			{
				TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date.Value;
				date = $"{ago:hh\\:mm\\:ss} ago";
			}

			// html-filtered map title
			string mapInfo = $"{beatmap.BeatmapSet.Artist} - {beatmap.BeatmapSet.Title} [{score.Beatmap.Version}]".FilterToHTML();

			string pp = $"| {score.Pp:N2}pp";
			if (beatmap.Mode == Mode.Osu)
			{
				try
				{
					// Add pp values
					var shouldCalculatePp = score.Pp is null ||
					                        score.ComboBasedMissCount(beatmap.MaxCombo, beatmap.Sliders) > 0;

					Pettanko.Difficulty.OsuDifficultyAttributes attributes = null;

					if (shouldCalculatePp)
					{
						var difficultyMods = score.Mods.Select(x => Pettanko.Mod.AllMods.FirstOrDefault(y => y.Acronym == x))
							.Select(x=> new Mod { Acronym = x.Acronym })
							.ToArray();

						attributes = await new GetBeatmapAttributes(beatmap.Id, difficultyMods).Execute();
					}

					double scorePp = score.Pp ?? PpCalculation.CalculatePerformance(score, attributes, beatmap);
					string possiblePp = string.Empty;

					if (score.ComboBasedMissCount(beatmap.MaxCombo, beatmap.Sliders) > 0)
					{
						// Add possible pp value if they missed
						var fcScore = new Score
						{
							Statistics = new Score.ScoreStatistics
							{
								Count300 = (score.Beatmap.ObjectsTotal - score.Statistics.Count100 - score.Statistics.Count50) ?? 0,
								Count100 = score.Statistics.Count100,
								Count50 = score.Statistics.Count50,
							},
							Combo = beatmap.MaxCombo,
							Mods = score.Mods
						};

						double possiblePPval = PpCalculation.CalculatePerformance(fcScore, attributes, beatmap);
						possiblePp = $"(~{possiblePPval:N2}pp if FC)";
					}

					pp = $"| {(score.Pp == null ? "~" : "")}{scorePp:N2}pp {possiblePp}";
				}
				catch (Exception e)
				{
					logger.LogError($"Oppai failed: {e.InnerMessageIfAny()}");
				}
			}

			var position = string.Empty;
			if (score.LeaderboardPosition != null)
				position = $"#{score.LeaderboardPosition}{(score.Mods.Length > 0 ? $" ({string.Join(null, score.Mods)})" : "") } | ";

			var completion = string.Empty;
			if (useAgo)
				completion = $" | {(double)(score.Statistics.Count300 + score.Statistics.Count100 + score.Statistics.Count50 + score.Statistics.CountMiss) / score.Beatmap.ObjectsTotal * 100.0:N1}% completion";

			return
				$"<b>({score.Grade.GetDescription()})</b> <a href=\"{score.Beatmap.Link}\">{mapInfo}</a><b>{mods} ({score.Accuracy:N2}%)</b>{Environment.NewLine}" +
				$"{score.Combo}/{beatmap.MaxCombo}x ({score.Statistics.Count300} / {score.Statistics.Count100} / {score.Statistics.Count50} / {score.Statistics.CountMiss}) {pp}{Environment.NewLine}" +
				$"{position}{date}{completion}{Environment.NewLine}{Environment.NewLine}";
		}

		private async Task<string> FormatLazerScore(LazerScore score, Beatmap beatmap, bool useAgo)
		{
			string mods = string.Empty;
			if (score.Mods.Count(x => x.Acronym != "CL") > 0)
			{
				mods = " +";
				foreach (var mod in score.Mods)
				{
					if (mod.Acronym != "DA")
						mods += mod.Acronym;

					if (mod.Settings is { Count: > 0 })
					{
						if (mod.Settings.TryGetValue("speed_change", out var rate))
						{
							mods += $"({double.Parse(rate!, CultureInfo.InvariantCulture)}x) ";
						}
						if (mod.Settings.TryGetValue("approach_rate", out var ar))
						{
							mods += $"AR{double.Parse(ar!, CultureInfo.InvariantCulture)} ";
						}
						if (mod.Settings.TryGetValue("circle_size", out var cs))
						{
							mods += $"CS{double.Parse(cs!, CultureInfo.InvariantCulture)} ";
						}
						if (mod.Settings.TryGetValue("overall_difficulty", out var od))
						{
							mods += $"OD{double.Parse(od!, CultureInfo.InvariantCulture)} ";
						}
					}
				}

				mods = mods.TrimEnd();
			}

			string date = score.Date?.ToShortDateString();
			if (useAgo && score.Date != null)
			{
				TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date.Value;
				date = $"{ago:hh\\:mm\\:ss} ago";
			}

			// html-filtered map title
			string mapInfo = $"{beatmap.BeatmapSet.Artist} - {beatmap.BeatmapSet.Title} [{score.Beatmap.Version}]".FilterToHTML();

			string pp = $"| {score.Pp:N2}pp";
			if (beatmap.Mode == Mode.Osu)
			{
				try
				{
					// Add pp values
					var shouldCalculatePp = score.Pp is null ||
											score.ComboBasedMissCount(beatmap.MaxCombo, beatmap.Sliders) > 0;

					Pettanko.Difficulty.OsuDifficultyAttributes attributes = null;

					if (shouldCalculatePp)
					{
						var difficultyMods = score.Mods.Where(x => Pettanko.Mod.AllMods.Any(y => y.Acronym == x.Acronym))
							.ToArray();
						attributes = await new GetBeatmapAttributes(beatmap.Id, difficultyMods).Execute();
					}

					double scorePp = score.Pp ?? PpCalculation.CalculatePerformance(score, attributes, beatmap);
					string possiblePp = string.Empty;

					if (score.ComboBasedMissCount(beatmap.MaxCombo, beatmap.Sliders) > 0)
					{
						// Add possible pp value if they missed
						var fcScore = new LazerScore
						{
							Statistics = new LazerScore.ScoreStatistics
							{
								Count300 = (score.Beatmap.ObjectsTotal - score.Statistics.Count100 - score.Statistics.Count50) ?? 0,
								Count100 = score.Statistics.Count100,
								Count50 = score.Statistics.Count50,
							},
							Combo = beatmap.MaxCombo,
							Mods = score.Mods
						};

						double possiblePPval = PpCalculation.CalculatePerformance(fcScore, attributes, beatmap);
						possiblePp = $"(~{possiblePPval:N2}pp if FC)";
					}

					pp = $"| {(score.Pp == null ? "~" : "")}{scorePp:N2}pp {possiblePp}";
				}
				catch (Exception e)
				{
					logger.LogError($"Oppai failed: {e.InnerMessageIfAny()}");
				}
			}

			var position = string.Empty;
			if (score.LeaderboardPosition != null)
				position = $"#{score.LeaderboardPosition} | ";

			var completion = string.Empty;
			if (useAgo)
				completion = $" | {(double)(score.Statistics.Count300 + score.Statistics.Count100 + score.Statistics.Count50 + score.Statistics.CountMiss) / score.Beatmap.ObjectsTotal * 100.0:N1}% completion";

			return
				$"<b>({score.Grade.GetDescription()})</b> <a href=\"{score.Beatmap.Link}\">{mapInfo}</a><b>{mods} ({score.Accuracy:N2}%)</b>{Environment.NewLine}" +
				$"{score.Combo}/{beatmap.MaxCombo}x ({score.Statistics.Count300} / {score.Statistics.Count100} / {score.Statistics.Count50} / {score.Statistics.CountMiss}) {pp}{Environment.NewLine}" +
				$"{position}{date}{completion}{Environment.NewLine}{Environment.NewLine}";
		}
	}
}