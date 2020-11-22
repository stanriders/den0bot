// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Modules.Osu.Osu.API.Requests.V2;
using den0bot.Modules.Osu.Osu.Types.V2;
using den0bot.Util;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu
{
	public class ModProfile : IModule, IReceiveAllMessages
	{
		private readonly Regex regex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly int topscores_to_show = 3;
		public ModProfile()
		{
			AddCommand(new Command
			{
				Name = "newppuser",
				ActionAsync = GetRebalanceProfile
			});
			Log.Debug("Enabled");
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
			{
				string playerID = GetPlayerIdFromMessage(message.Text);
				if (!string.IsNullOrEmpty(playerID))
					await API.SendMessage(await FormatPlayerInfo(playerID), message.Chat.Id, ParseMode.Html,
											message.MessageId, null, false);
			}
		}

		private string GetPlayerIdFromMessage(string text)
		{
			Match regexMatch = regex.Match(text);
			if (regexMatch.Groups.Count > 1)
			{
				return regexMatch.Groups[1].Value;
			}

			return string.Empty;
		}

		private async Task<string> FormatPlayerInfo(string playerID)
		{
			Osu.Types.V2.User info = await Osu.WebApi.MakeApiRequest(new GetUser(playerID));

			if (info == null)
				return string.Empty;

			List<Score> topscores = await Osu.WebApi.MakeApiRequest(new GetUserScores(info.Id.ToString(), ScoreType.Best));

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
					$"<b>{(i + 1)}</b>. <code>{score.Pp:F1}pp</code> | {mapName}{mods} (<b>{score.Grade.GetDescription()}</b>, {score.Accuracy:N2}%)\n";
			}

			var title = string.Empty;
			if (!string.IsNullOrEmpty(info.Title))
				title = $"<i>{info.Title}</i>\n";

			return $"<b>{info.Username}</b> <a href=\"{info.AvatarUrl}\">-</a> <code>{info.Statistics.Pp}pp</code> - #{info.Statistics.Rank.Global} (#{info.Statistics.Rank.Country} {info.Country.Code})\n" +
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
				var playerJson = await Web.DownloadString($"https://newpp.stanr.info/api/GetResults?player={username}");
				if (!string.IsNullOrEmpty(playerJson))
				{
					dynamic player = JsonConvert.DeserializeObject(playerJson);

					string formattedTopscores = string.Empty;
					for (int i = 0; i < topscores_to_show; i++)
					{
						var map = string.Join(" - ", player.Beatmaps[i].Beatmap.ToString().Split(" - ").Skip(1));
						formattedTopscores +=
							$"{map} | {player.Beatmaps[i].LocalPP}pp ({player.Beatmaps[i].PPChange})\n";
					}

					return $"{player.Username}\nLive PP: {player.LivePP}\nLocal PP: {player.LocalPP}\n__________\n{formattedTopscores}";
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
			return string.Empty;
		}
		private async Task<string> GetRebalanceProfile(Message msg)
		{
			if (!string.IsNullOrEmpty(msg.Text))
			{
				if (msg.Text.Length > 11)
				{
					string player = GetPlayerIdFromMessage(msg.Text);
					if (!string.IsNullOrEmpty(player))
					{
						var result = await FormatRebalanceProfile(player);
						if (!string.IsNullOrEmpty(result))
							return result;
					}
				}
				else
				{
					var osuId = DatabaseOsu.GetPlayerOsuIDFromDatabase(msg.From.Id);
					if (osuId != 0)
					{
						var result = await FormatRebalanceProfile(osuId.ToString());
						if (!string.IsNullOrEmpty(result))
							return result;
					}
				}
			}

			return Localization.Get("generic_badrequest", msg.Chat.Id);
		}
	}
}
