// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using den0bot.Osu.API.Requests;
using den0bot.Osu.Types;
using den0bot.Util;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModMaplist : IModule
	{
		private readonly List<string[]> maplist = new List<string[]>();
		private readonly string spreadsheet = "1AxoXTpNjFnWsFPuSa8rlMbtSnMtkYnKZyzUY_4FTbig";

		private bool isEnabled;

		public ModMaplist()
		{
			Start();
			AddCommand(new Command()
			{
				Name = "map",
				ActionAsync = GetMap
			});
			Log.Debug($"Enabled, {maplist.Count} maps");
		}

		private bool Start()
		{
			if (string.IsNullOrEmpty(Config.Params.GoogleAPIToken))
				return false;

			Log.Debug("Loading...");
			try
			{
				string request = "https://sheets.googleapis.com/v4/spreadsheets/" + spreadsheet +
				                 "/values/A2:B200?key=" + Config.Params.GoogleAPIToken;
				var data = new WebClient().DownloadString(request);

				JArray array = JToken.Parse(data)["values"] as JArray;

				foreach (JToken token in array)
				{
					maplist.Add(new string[] {token[0].ToString(), token[1].ToString()});
				}

				isEnabled = true;
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to start: " + ex.InnerMessageIfAny());
				return false;
			}
		}

		private async Task<string> GetMap(Telegram.Bot.Types.Message message)
		{
			if (isEnabled && maplist.Count > 0)
			{
				int num = RNG.Next(max: maplist.Count);
				string link = maplist[num][1].Substring(19);
				Map map = null;
				if (link[0] == 's')
				{
					List<Map> set = await Osu.WebApi.MakeAPIRequest(new GetBeatmapSet
					{
						ID = uint.Parse(link.Substring(2))
					});
					map = set?.Last();
				}
				else if (link[0] == 'b')
				{
					map = await Osu.WebApi.MakeAPIRequest(new GetBeatmap
					{
						ID = uint.Parse(link.Substring(2))
					});
				}
				else
				{
					return maplist[num][0] + Environment.NewLine + maplist[num][1];
				}

				if (map != null)
				{
					string format = ModBeatmap.FormatMapInfo(map, Mods.None);
					string caption = $"{maplist[num][0]} {format}";
					if (caption.Length > 265) // 200 regular character limit + HTML
					{
						if (caption.Length - maplist[num][0].Length > 265)
							caption =
								$"<a href=\"{maplist[num][1]}\">{maplist[num][0]}</a>"; // shouldn't happen really, but who knows
						else
							caption = $"{format}";
					}

					API.SendPhoto(map?.Thumbnail, message.Chat, caption, ParseMode.Html);
				}

				return string.Empty;
			}
			else
			{
				Log.Info("Trying to start again");
				if (!Start())
					return string.Empty;
			}

			return string.Empty;
		}
	}
}