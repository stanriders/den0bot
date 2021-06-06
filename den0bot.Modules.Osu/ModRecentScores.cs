// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.WebAPI;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.Util;
using den0bot.Util;
using Telegram.Bot.Types.Enums;
using den0bot.Types;
using den0bot.Types.Answers;
using Serilog;

namespace den0bot.Modules.Osu
{
	public class ModRecentScores : OsuModule
	{
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
		}

		private async Task<ICommandAnswer> GetScores(Telegram.Bot.Types.Message message)
		{
			string playerID;
			int amount = 1;

			List<string> msgSplit = message.Text.Split(' ').ToList();
			msgSplit.RemoveAt(0);

			if (msgSplit.Count > 0 && int.TryParse(msgSplit.Last(), out amount))
			{
				if (amount > recent_amount)
					amount = recent_amount;

				msgSplit.Remove(msgSplit.Last());
			}

			if (msgSplit.Count > 0)
			{
				playerID = string.Join(" ", msgSplit);
			}
			else
			{
				await using (var db = new DatabaseOsu())
				{
					var id = db.Players.FirstOrDefault(x=> x.TelegramID == message.From.Id)?.OsuID;
					if (id == null || id == 0)
						return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

					playerID = id.ToString();
				}
			}

			List<Score> lastScores = await WebApiHandler.MakeApiRequest(new GetUserScores(playerID, ScoreType.Recent, true));
			if (lastScores != null)
			{
				if (lastScores.Count == 0)
					return Localization.GetAnswer("recentscores_no_scores", message.Chat.Id);

				string result = string.Empty;
				foreach (var score in lastScores.Take(amount))
				{
					if (amount == 1)
						ChatBeatmapCache.StoreMap(message.Chat.Id, score.BeatmapShort.Id);

					score.Beatmap = await WebApiHandler.MakeApiRequest(new GetBeatmap(score.BeatmapShort.Id));
					result += FormatScore(score, true);
				}

				return new TextCommandAnswer(result);
			}

			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> GetPass(Telegram.Bot.Types.Message message)
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
				await using (var db = new DatabaseOsu())
				{
					var id = db.Players.FirstOrDefault(x => x.TelegramID == message.From.Id)?.OsuID;
					if (id == null || id == 0)
						return Localization.GetAnswer("recentscores_unknown_player", message.Chat.Id);

					playerID = id.ToString();
				}
			}

			var lastScores = await WebApiHandler.MakeApiRequest(new GetUserScores(playerID, ScoreType.Recent, false));
			if (lastScores.Count > 0)
			{
				var score = lastScores[0];
				ChatBeatmapCache.StoreMap(message.Chat.Id, score.BeatmapShort.Id);

				score.Beatmap = await WebApiHandler.MakeApiRequest(new GetBeatmap(score.BeatmapShort.Id));
				return new TextCommandAnswer(FormatScore(score, true));
			}

			return Localization.GetAnswer("generic_fail", message.Chat.Id);
		}

		private async Task<ICommandAnswer> GetMapScores(Telegram.Bot.Types.Message message)
		{
			var messageToEdit = await API.SendMessage(Localization.Get("generic_wait", message.Chat.Id), message.Chat.Id, ParseMode.Html, message.MessageId);
			if (messageToEdit == null)
				return null;

			// beatmap id regex can parse link as part of a complex message so we dont need to clean it up beforehand
			var msgText = message.Text;
			if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains(".ppy.sh"))
				msgText = message.ReplyToMessage.Text; // replies to beatmaps should return scores for these beatmaps

			var msgSplit = msgText.Split(' ').ToList();

			uint mapId = 0;
			var mods = LegacyMods.NM;
			if (msgSplit.Count > 1)
			{
				var data = BeatmapLinkParser.Parse(msgText);
				if (data != null)
				{
					mapId = data.ID;
					mods = data.Mods;
					if (data.IsBeatmapset)
					{
						BeatmapSet set = await WebApiHandler.MakeApiRequest(new GetBeatmapSet(data.ID));
						if (set?.Beatmaps?.Count > 0)
							mapId = set.Beatmaps.OrderBy(x => x.StarRating).Last().Id;
					}
				}
			}
			else
			{
				mapId = ChatBeatmapCache.GetMap(message.Chat.Id);
			}

			if (mapId == 0)
			{
				await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, Localization.Get("generic_fail", message.Chat.Id), parseMode: ParseMode.Html);
				return null;
			}

			await using var db = new DatabaseOsu();

			var playerId = db.Players.FirstOrDefault(x => x.TelegramID == message.From.Id)?.OsuID;
			if (playerId == null || playerId == 0)
			{
				await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, Localization.Get("recentscores_unknown_player", message.Chat.Id), parseMode: ParseMode.Html);
				return null;
			}

			var result = string.Empty;

			if (mods == 0)
			{
				// no mods specified - use apiv1 to get all scores on a map and then get score data from apiv2
				var scores = await WebApiHandler.MakeApiRequest(new WebAPI.Requests.V1.GetScores(playerId.Value.ToString(), mapId, mods, score_amount));
				if (scores == null || scores.Count <= 0)
				{
					await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, Localization.Get("recentscores_no_scores", message.Chat.Id), parseMode: ParseMode.Html);
					return null;
				}

				var map = await WebApiHandler.MakeApiRequest(new GetBeatmap(mapId));

				foreach (var v1Score in scores)
				{
					var score = await WebApiHandler.MakeApiRequest(new GetUserBeatmapScore(mapId, playerId.Value, v1Score.LegacyMods));
					if (score != null)
					{
						score.Beatmap = map;
						result += FormatScore(score, false);
					}
				}
			}
			else
			{
				// mods specified - get data straight from apiv2
				var score = await WebApiHandler.MakeApiRequest(new GetUserBeatmapScore(mapId, playerId.Value, mods));
				if (score == null)
				{
					await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, Localization.Get("recentscores_no_scores", message.Chat.Id), parseMode: ParseMode.Html);
					return null;
				}

				score.Beatmap = await WebApiHandler.MakeApiRequest(new GetBeatmap(mapId));
				result += FormatScore(score, false);
			}
				
			if (!string.IsNullOrEmpty(result))
			{
				ChatBeatmapCache.StoreMap(message.Chat.Id, mapId);
				await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, result, parseMode: ParseMode.Html);
				return null;
			}

			await API.EditMessage(message.Chat.Id, messageToEdit.MessageId, Localization.Get("generic_fail", message.Chat.Id), parseMode: ParseMode.Html);
			return null;
		}

		private async Task<ICommandAnswer> AddMe(Telegram.Bot.Types.Message message)
		{
			if (message.Text.Length > 6)
			{
				await using (var db = new DatabaseOsu())
				{
					string player = ProfileLinkParser.Parse(message.Text)?.Id;
					if (string.IsNullOrEmpty(player))
						player = message.Text.Substring(7);

					if (!string.IsNullOrEmpty(player))
					{
						if(db.Players.Any(x=> x.TelegramID == message.From.Id))
							return Localization.GetAnswer($"annoy_{RNG.NextNoMemory(1, 10)}", message.Chat.Id);

						if (!uint.TryParse(player, out var osuID))
						{
							// if they used /u/cookiezi instead of /u/124493 we ask osu API for an ID
							var info = await WebApiHandler.MakeApiRequest(new GetUser(player));

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

		private static string FormatScore(IScore score, bool useAgo)
		{
			string mods = string.Empty;
			if (score.LegacyMods != LegacyMods.NM)
				mods = $" +{score.LegacyMods?.ReadableMods()}";

			string date = score.Date?.ToShortDateString();
			if (useAgo && score.Date != null)
			{
				TimeSpan ago = DateTime.Now.ToUniversalTime() - score.Date.Value;
				date = $"{ago:hh\\:mm\\:ss} ago";
			}

			Beatmap beatmap = (Beatmap)score.Beatmap;
			// html-filtered map title
			string mapInfo = $"{beatmap.BeatmapSet.Artist} - {beatmap.BeatmapSet.Title} [{score.Beatmap.Version}]".FilterToHTML();

			string pp = $"| {score.Pp:N2}pp";
			if (beatmap.Mode == Mode.Osu)
			{
				try
				{
					// Add pp values
					double scorePp = score.Pp ?? Oppai.GetBeatmapPP(score.Beatmap, score);
					string possiblePp = string.Empty;

					if (score.ComboBasedMissCount(beatmap.MaxCombo.Value, beatmap.Sliders.Value) > 0)
					{
						// Add possible pp value if they missed
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
						possiblePp = $"(~{possiblePPval:N2}pp if FC)";
					}

					pp = $"| {(score.Pp == null ? "~" : "")}{scorePp:N2}pp {possiblePp}";
				}
				catch (Exception e)
				{
					Log.Error($"Oppai failed: {e.InnerMessageIfAny()}");
				}
			}

			var position = string.Empty;
			if (score.LeaderboardPosition != null)
				position = $"#{score.LeaderboardPosition}{(!string.IsNullOrEmpty(mods) ? $" ({score.LegacyMods?.ReadableMods()})" : "") } | ";

			var completion = string.Empty;
			if (useAgo)
				completion = $" | {(double)(score.Count300 + score.Count100 + score.Count50 + score.Misses) / score.Beatmap.ObjectsTotal * 100.0:N1}% completion";

			return
				$"<b>({score.Grade.GetDescription()})</b> <a href=\"{score.Beatmap.Link}\">{mapInfo}</a><b>{mods} ({score.Accuracy:N2}%)</b>{Environment.NewLine}" +
				$"{score.Combo}/{beatmap.MaxCombo}x ({score.Count300} / {score.Count100} / {score.Count50} / {score.Misses}) {pp}{Environment.NewLine}" +
				$"{position}{date}{completion}{Environment.NewLine}{Environment.NewLine}";
		}
	}
}