// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.Types;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Util;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using den0bot.Modules.Osu.WebAPI;
using den0bot.Types;
using den0bot.Modules.Osu.Util;
using den0bot.Types.Answers;
using Serilog;
using Microsoft.Extensions.Logging;

namespace den0bot.Modules.Osu
{
	public class ModMaplist : OsuModule
	{
		private readonly List<string[]> maplist = new();
		private readonly string spreadsheet = "1AxoXTpNjFnWsFPuSa8rlMbtSnMtkYnKZyzUY_4FTbig";

		private bool isEnabled;

		public ModMaplist(ILogger<IModule> logger) : base(logger)
		{
			AddCommand(new Command
			{
				Name = "map",
				ActionAsync = GetMap,
				ParseMode = ParseMode.Html
			});
		}

		public override bool Init()
		{
			if (string.IsNullOrEmpty(Config.Params.GoogleAPIToken))
				return false;

			Start(); // not using returned value to be able to restart later
			return base.Init();
		}

		private bool Start()
		{
			Log.Debug("Loading...");
			try
			{
				string request = "https://sheets.googleapis.com/v4/spreadsheets/" + spreadsheet +
				                 "/values/A2:B200?key=" + Config.Params.GoogleAPIToken;
				var data = Web.DownloadString(request).Result;

				JArray array = JToken.Parse(data)["values"] as JArray;

				foreach (JToken token in array)
				{
					maplist.Add(new string[] {token[0].ToString(), token[1].ToString()});
				}

				isEnabled = true;

				Log.Debug($"Loaded {maplist.Count} maps");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to start: " + ex.InnerMessageIfAny());
				return false;
			}
		}

		private async Task<ICommandAnswer> GetMap(Telegram.Bot.Types.Message message)
		{
			if (isEnabled && maplist.Count > 0)
			{
				int num = RNG.Next(max: maplist.Count);
				var linkData = BeatmapLinkParser.Parse(maplist[num][1]);
				if (linkData == null)
					return new TextCommandAnswer(maplist[num][0] + Environment.NewLine + maplist[num][1]);

				Beatmap map;
				if (linkData.IsBeatmapset)
				{
					var set = await new GetBeatmapSet(linkData.ID).Execute();
					map = set?.Beatmaps.Last();
				}
				else
				{
					map = await new GetBeatmap(linkData.ID).Execute();
				}

				if (map != null)
				{
					string format = await map.GetFormattedMapInfo();
					string caption = $"{maplist[num][0]} {format}";
					if (caption.Length > 265) // 200 regular character limit + HTML
					{
						if (caption.Length - maplist[num][0].Length > 265)
							caption =
								$"<a href=\"{maplist[num][1]}\">{maplist[num][0]}</a>"; // shouldn't happen really, but who knows
						else
							caption = $"{format}";
					}

					return new ImageCommandAnswer
					{
						Image = map.BeatmapSet.Covers.Cover2X,
						Caption = caption
					};
				}

				return null;
			}
			else
			{
				Log.Information("Trying to start again");
				if (!Start())
					return null;
			}

			return null;
		}
	}
}