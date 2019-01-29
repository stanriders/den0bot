// den0bot (c) StanR 2019 - MIT License
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Osu;
using den0bot.Util;

namespace den0bot.Modules
{
	class ModProfile : IModule, IReceiveAllMessages
	{
		private readonly Regex regex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly int topscores_to_show = 3;
		public ModProfile() { Log.Debug(this, "Enabled"); }

		public async void ReceiveMessage(Message message)
		{
			Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
				string playerID = regexMatch.Groups[1]?.Value;
				if (!string.IsNullOrEmpty(playerID))
					API.SendMessage( await FormatPlayerInfo(playerID), message.Chat, ParseMode.Html, message.MessageId, null, false);
			}
		} 

		private async Task<string> FormatPlayerInfo(string playerID)
		{
			Player info = await OsuAPI.GetPlayerAsync(playerID);
			if (info == null)
				return string.Empty;

			List<Score> topscores = await OsuAPI.GetTopscoresAsync(info.ID, topscores_to_show);
			if (topscores == null || topscores.Count <= 0)
				return string.Empty;

			string formatedTopscores = string.Empty;

			for (int i = 0; i < topscores.Count; i++)
			{
				Score score = topscores[i];
				Map map = await OsuAPI.GetBeatmapAsync(score.BeatmapID);

				string mods = string.Empty;
				Mods enabledMods = score.EnabledMods ?? Mods.None;
				if (enabledMods > 0)
					mods = " +" + enabledMods.ToString().Replace(", ", "");

				// 1. Artist - Title [Diffname] +Mods (Rank, Accuracy%) - 123pp
				string mapName = $"{map.Artist} - {map.Title} [{map.Difficulty}]".FilterToHTML();

				formatedTopscores +=
					$"<b>{(i + 1)}</b>. {mapName}{mods} (<b>{score.Rank}</b>, {score.Accuracy.FN2()}%) - <b>{score.Pp}</b>pp\n";
				
			}

			return $"<b>{info.Username}</b> <a href=\"https://a.ppy.sh/{info.ID}_0.jpeg\">-</a> #{info.Rank} ({info.Pp}pp)\nPlaycount: {info.Playcount}\n__________\n{formatedTopscores}";
		}

	}
}
