﻿// den0bot (c) StanR 2025 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Util;
using Telegram.Bot.Types.Enums;
using den0bot.Types;
using den0bot.Types.Answers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

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
					ActionAsync = m => GetLastScores(m, false),
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Names = {"lastpass", "lp"},
					Reply = true,
					Slow = true,
					ActionAsync = m => GetLastScores(m, true),
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

		private async Task<ICommandAnswer> GetLastScores(Telegram.Bot.Types.Message message, bool passesOnly)
		{
			uint playerId = 0;
			int amount = 1;

			List<string> msgSplit = message.Text!.Split(' ').ToList();
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
					playerId = (uint)player.OnlineID;
			}
			else
			{
				await using var db = new DatabaseOsu();

				var id = db.Players.AsNoTracking().FirstOrDefault(x=> x.TelegramID == message.From!.Id)?.OsuID;
				if (id == null || id == 0)
					return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

				playerId = id.Value;
			}

			if (playerId == 0)
				return Localization.GetAnswer("generic_fail", message.Chat.Id);

			List<Score>? lastScores = await new GetUserScores((int)playerId, ScoreType.Recent, !passesOnly, amount).Execute();
			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				string result = string.Empty;
				var fetchedMaps = new List<Beatmap>();
				foreach (var score in lastScores.Take(amount))
				{
					if (score.Beatmap != null)
					{
						if (amount == 1)
						{
							ChatBeatmapCache.StoreLastMap(message.Chat.Id, new ChatBeatmapCache.CachedBeatmap
								{ BeatmapId = score.Beatmap.OnlineID, BeatmapSetId = score.Beatmap.OnlineBeatmapSetID });
						}

						var beatmap = fetchedMaps.FirstOrDefault(x => x.OnlineID == score.Beatmap.OnlineID);
						if (beatmap == null)
						{
							var apiBeatmap = await new GetBeatmap(score.Beatmap.OnlineID).Execute();
							if (apiBeatmap != null)
							{
								beatmap = apiBeatmap;
								fetchedMaps.Add(apiBeatmap);
							}
						}

						result += FormatScore(score, beatmap, true);
					}
				}

				return new TextCommandAnswer(result);
			}

			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> GetMapScores(Telegram.Bot.Types.Message message)
		{
			int mapId = 0;

			// beatmap id regex can parse link as part of a complex message so we dont need to clean it up beforehand
			var msgText = message.Text!;
			if (message.ReplyToMessage?.Text != null)
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
						var set = await new GetBeatmapSet(data.ID).Execute();
						if (set?.Beatmaps.Length > 0)
							mapId = set.Beatmaps.OrderBy(x => x.StarRating).Last().OnlineID;
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

			var playerId = db.Players.AsNoTracking().FirstOrDefault(x => x.TelegramID == message.From!.Id)?.OsuID;
			if (playerId == null || playerId == 0)
				return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

			var result = string.Empty;

			var map = await new GetBeatmap(mapId).Execute();

			if (mods.Length == 0)
			{
				var scores = await new GetUserBeatmapScores(mapId, playerId.Value).Execute();
				if (scores == null || scores.Count <= 0)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);


				foreach (var score in scores.DistinctBy(x => string.Join("", x.Mods.Select(m=> m.Acronym)))
					         .OrderByDescending(x => x.TotalScore)
					         .Take(score_amount))
				{
					result += FormatScore(score, map, false);
				}
			}
			else
			{
				var score = await new GetUserBeatmapScore(mapId, playerId.Value, mods.Select(x=> x.Acronym).ToArray()).Execute();
				if (score == null)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				result += FormatScore(score, map, false);
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
			if (message.Text?.Length > 6)
			{
				await using var db = new DatabaseOsu();

				string? player = ProfileLinkParser.Parse(message.Text!)?.Id;
				if (string.IsNullOrEmpty(player))
					player = message.Text.Substring(7);

				if (!string.IsNullOrEmpty(player))
				{
					if (db.Players.Any(x => x.TelegramID == message.From!.Id))
						return Localization.GetAnswer($"annoy_{RNG.NextNoMemory(1, 10)}", message.Chat.Id);

					if (!uint.TryParse(player, out var osuID))
					{
						// if they used /u/cookiezi instead of /u/124493 we ask osu API for an ID
						var info = await new GetUser(player).Execute();
						if (info == null)
							return Localization.GetAnswer("recentscores_player_add_failed", message.Chat.Id);

						osuID = (uint)info.OnlineID;
					}

					if (osuID != 0)
					{
						await db.Players.AddAsync(new DatabaseOsu.Player
						{
							OsuID = osuID,
							TelegramID = message.From!.Id
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
			await using var db = new DatabaseOsu();

			var player = db.Players.FirstOrDefault(x=> x.TelegramID == message.From!.Id);
			if (player != null)
			{
				db.Players.Remove(player);
				await db.SaveChangesAsync();

				return Localization.GetAnswer("recentscores_player_remove_success", message.Chat.Id);
			}
				
			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> RemovePlayer(Telegram.Bot.Types.Message message)
		{
			await using var db = new DatabaseOsu();

			var tgId = DatabaseCache.GetUserID(message.Text!.Split()[1]);
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

		private string FormatScore(Score score, Beatmap? beatmap, bool useAgo)
		{
			string mods = string.Empty;
			if (score.Mods.Count(x => x.Acronym != "CL") > 0)
			{
				mods = " +";
				foreach (var mod in score.Mods.Where(x => x.Acronym != "CL"))
				{
					if (mod.Acronym != "DA")
						mods += mod.Acronym;

					if (mod.Settings is { Count: > 0 })
					{
						if (mod.Settings.TryGetValue("speed_change", out var rate))
						{
							mods += $"({(double)rate}x) ";
						}
						if (mod.Settings.TryGetValue("approach_rate", out var ar))
						{
							mods += $"AR{(double)ar} ";
						}
						if (mod.Settings.TryGetValue("circle_size", out var cs))
						{
							mods += $"CS{(double)cs} ";
						}
						if (mod.Settings.TryGetValue("overall_difficulty", out var od))
						{
							mods += $"OD{(double)od} ";
						}
					}
				}

				mods = mods.TrimEnd();
			}

			string? date = score.Date?.ToShortDateString();
			if (useAgo && score.Date != null)
			{
				TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date.Value;
				date = $"{ago:hh\\:mm\\:ss} ago";
			}

			// html-filtered map title
			string mapInfo = $"{beatmap?.BeatmapSet?.Artist} - {beatmap?.BeatmapSet?.Title} [{beatmap?.DifficultyName}]".FilterToHTML();

			string mapDifficulty = "";
			string pp = $"| {score.Pp:N2}pp";
			if (beatmap != null && beatmap.MaxCombo != null)
			{
				try
				{
					var attributes = PpCalculation.CalculateDifficulty(score.Mods, beatmap);
					if (attributes != null)
					{
						mapDifficulty = $"<b>[{attributes.StarRating:N1}*]</b> ";
						// Add pp values
						var shouldCalculatePp = score.Pp is null ||
						                        score.ComboBasedMissCount(beatmap.MaxCombo.Value, beatmap.SliderCount) > 0;

						double scorePp = score.Pp ?? 0;
						if (shouldCalculatePp)
						{
							scorePp = score.Pp ??
							          PpCalculation.CalculatePerformance(score.ToScoreInfo(), attributes, beatmap) ?? 0;
						}

						string possiblePp = string.Empty;

						if (score.ComboBasedMissCount(beatmap.MaxCombo.Value, beatmap.SliderCount) > 0)
						{
							// Add possible pp value if they missed
							var serialized = JsonConvert.SerializeObject(score);
							var fcScore = JsonConvert.DeserializeObject<Score>(serialized);

							var greats = Math.Max(0, beatmap.TotalObjectCount -
								score.Statistics.GetValueOrDefault(HitResult.Ok) -
								score.Statistics.GetValueOrDefault(HitResult.Meh));

							fcScore!.Statistics[HitResult.Great] = greats;
							fcScore.Statistics[HitResult.Miss] = 0;
							fcScore.Combo = beatmap.MaxCombo.Value;
							fcScore.Accuracy = PpCalculation.GetAccuracyForRuleset(beatmap, fcScore.Statistics);

							double possiblePPval =
								PpCalculation.CalculatePerformance(fcScore.ToScoreInfo(), attributes, beatmap) ?? 0;
							possiblePp = $"(~{possiblePPval:N2}pp if FC)";
						}

						pp = $"| {(score.Pp == null ? "~" : "")}{scorePp:N2}pp {possiblePp}";
					}
				}
				catch (Exception e)
				{
					logger.LogError($"PP calculation failed: {e.InnerMessageIfAny()}");
				}
			}

			var position = string.Empty;
			if (score.LeaderboardPosition != null)
				position = $"#{score.LeaderboardPosition} | ";

			var completion = string.Empty;
			if (useAgo)
				completion = $" | {(double)(score.Statistics.GetValueOrDefault(HitResult.Great) + score.Statistics.GetValueOrDefault(HitResult.Ok) + score.Statistics.GetValueOrDefault(HitResult.Meh) + score.Statistics.GetValueOrDefault(HitResult.Miss)) / score.Beatmap?.TotalObjectCount * 100.0:N1}% completion";

			return
				$"<b>({(score.Passed ? score.Grade.GetDescription() : "F")})</b> <a href=\"{beatmap?.Link}\">{mapDifficulty}{mapInfo}</a><b>{mods} ({score.Accuracy:N2}%)</b>{Environment.NewLine}" +
				$"{score.Combo}/{beatmap?.MaxCombo}x ({score.Statistics.GetValueOrDefault(HitResult.Great)} / {score.Statistics.GetValueOrDefault(HitResult.Ok)} / {score.Statistics.GetValueOrDefault(HitResult.Meh)} / {score.Statistics.GetValueOrDefault(HitResult.Miss)}) {pp}{Environment.NewLine}" +
				$"{position}{date}{completion}{Environment.NewLine}{Environment.NewLine}";
		}
	}
}