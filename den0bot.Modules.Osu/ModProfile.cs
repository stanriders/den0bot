// den0bot (c) StanR 2021 - MIT License
using System;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.Types;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Util;
using Newtonsoft.Json;
using den0bot.Modules.Osu.WebAPI;
using Microsoft.EntityFrameworkCore;
using den0bot.Types;
using den0bot.Types.Answers;
using Serilog;

namespace den0bot.Modules.Osu
{
	public class ModProfile : OsuModule, IReceiveAllMessages
	{
		private readonly int topscores_to_show = 5;
		public ModProfile()
		{
			AddCommands(new []
			{
				new Command
				{
					Name = "profile",
					ActionAsync = GetProfile,
					ParseMode = ParseMode.Html
				},
				new Command
				{
					Name = "newppuser",
					ActionAsync = GetRebalanceProfile
				}
			});
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
			{
				string playerId = ProfileLinkParser.Parse(message.Text)?.Id;
				if (!string.IsNullOrEmpty(playerId))
				{
					await API.SendMessage(await FormatPlayerInfo(playerId), message.Chat.Id, ParseMode.Html,
						message.MessageId, disablePreview: false);
				}
			}
		}

		private async Task<string> FormatPlayerInfo(string playerID)
		{
			Types.V2.User info = await WebApiHandler.MakeApiRequest(new GetUser(playerID));

			if (info == null)
				return string.Empty;

			var topscores = await WebApiHandler.MakeApiRequest(new GetUserScores(info.Id, ScoreType.Best, limit: topscores_to_show));
			if (topscores == null || topscores.Count <= 0)
				return string.Empty;

			string formatedTopscores = string.Empty;

			for (int i = 0; i < topscores.Count; i++)
			{
				Score score = topscores[i];

				string mods = string.Empty;
				if (score.Mods.Length > 0)
					mods = $" +{string.Join("", score.Mods)}";

				// 1. 123pp | Artist - Title [Diffname] +Mods (Rank, Accuracy%)
				string mapName = $"{score.BeatmapSet.Artist} - {score.BeatmapSet.Title} [{score.Beatmap.Version}]".FilterToHTML();
				formatedTopscores +=
					$"<b>{(i + 1)}</b>. <code>{score.Pp:F1}pp</code> | (<b>{score.Grade.GetDescription()}</b>) {mapName}{mods} ({score.Accuracy:N2}%)\n";
			}

			var title = string.Empty;
			if (!string.IsNullOrEmpty(info.Title))
				title = $"<i>{info.Title}</i>\n";

			return $"<b>{info.Username}</b> <a href=\"{info.AvatarUrl}\">-</a> <code>{info.Statistics.Pp}pp</code> - #{info.Statistics.GlobalRank} (#{info.Statistics.Rank.Country} {info.Country.Code})\n" +
				   $"{title}\n" +
				   $"<b>Playcount</b>: {info.Statistics.Playcount} ({info.Statistics.PlaytimeSeconds / 60.0 / 60.0:F2} hours)\n" +
			       $"<b>Joined on</b>: {info.JoinDate}\n" +
			       $"__________\n" +
			       $"{formatedTopscores}";
		}

		private async Task<string> FormatRebalanceProfile(string username)
		{
			try
			{
				var playerJson = await Web.DownloadString($"https://newpp.stanr.info/api/player/{username}");
				if (!string.IsNullOrEmpty(playerJson))
				{
					dynamic player = JsonConvert.DeserializeObject(playerJson);
					if (player != null)
					{
						string formattedTopscores = string.Empty;
						for (int i = 0; i < topscores_to_show; i++)
						{
							formattedTopscores +=
								$"{player.Scores[i].Map.Name} +{player.Scores[i].Mods} | {player.Scores[i].LocalPp}pp\n";
						}

						return
							$"{player.Name}\nLive PP: {player.LivePp}\nLocal PP: {player.LocalPp}\n__________\n{formattedTopscores}";
					}
				}
			}
			catch (Exception e)
			{
				Log.Warning($"FormatRebalanceProfile failed: {e}");
			}

			return string.Empty;
		}

		private async Task<ICommandAnswer> GetRebalanceProfile(Message msg)
		{
			if (!string.IsNullOrEmpty(msg.Text))
			{
				await using var db = new DatabaseOsu();

				string player;
				if (msg.Text.Length > 11)
					player = ProfileLinkParser.Parse(msg.Text)?.Id;
				else
					player = (await db.Players.FirstOrDefaultAsync(x => x.TelegramID == msg.From.Id))?.ToString();

				if (!string.IsNullOrEmpty(player))
				{
					var result = await FormatRebalanceProfile(player);
					if (!string.IsNullOrEmpty(result))
						return new TextCommandAnswer(result);
				}
			}

			return Localization.GetAnswer("generic_badrequest", msg.Chat.Id);
		}

		private async Task<ICommandAnswer> GetProfile(Message msg)
		{
			if (!string.IsNullOrEmpty(msg.Text))
			{
				await using var db = new Database();
				await using var dbOsu = new DatabaseOsu();

				var userId = msg.From.Id;

				if (msg.ReplyToMessage != null)
				{
					userId = msg.ReplyToMessage.From.Id;
				}

				// username in profile has priority over replies
				if (msg.Text.Length > 9)
				{
					var username = msg.Text.Substring(9).Trim();
					if (username.StartsWith('@'))
						username = username[1..];

					var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username);
					if (user != null)
						userId = user.TelegramID;
				}

				var player = await dbOsu.Players.FirstOrDefaultAsync(x => x.TelegramID == userId);
				if (player != null)
				{
					var result = await FormatPlayerInfo(player.OsuID.ToString());
					if (!string.IsNullOrEmpty(result))
						return new TextCommandAnswer(result);
				}
			}

			return Localization.GetAnswer("generic_badrequest", msg.Chat.Id);
		}
	}
}
