// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using den0bot.Osu;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
    class ModMaplist : IModule
    {
        private List<string[]> Maplist = new List<string[]>();
        private readonly string spreadsheet = "1AxoXTpNjFnWsFPuSa8rlMbtSnMtkYnKZyzUY_4FTbig";

        private bool isEnabled = false;

        public ModMaplist()
        {
            Start();
            AddCommand(new Command()
            {
                Name = "map",
                ActionAsync = (msg) => GetMap(msg)
            });
            Log.Info(this, $"Enabled, {Maplist.Count} maps");
        }

        private bool Start()
        {
			if (string.IsNullOrEmpty(Config.googleapi_token))
				return false;

			Log.Info(this, "Loading...");
            try
            {
                string request = "https://sheets.googleapis.com/v4/spreadsheets/" + spreadsheet + "/values/A2:B999?key=" + Config.googleapi_token;
                var data = new WebClient().DownloadData(request);

                JArray array = JToken.Parse(Encoding.UTF8.GetString(data))["values"] as JArray;

                foreach (JToken token in array)
                {
                    Maplist.Add(new string[] { token[0].ToString(), token[1].ToString() });
                }

                isEnabled = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(this, "Failed to start: " + ex.InnerMessageIfAny());
                return false;
            }
        }

        public async Task<string> GetMap(Telegram.Bot.Types.Message message)
        {
            if (isEnabled && Maplist.Count > 0)
            {
                int num = RNG.Next(Maplist.Count);
                string temp = Maplist[num][1].Substring(19);
                Map map = null;
                if (temp[0] == 's')
                {
                    List<Map> set = await OsuAPI.GetBeatmapSetAsync(uint.Parse(temp.Substring(2)));
                    map = set?.Last();
                }
                else if (temp[0] == 'b')
                {
                    map = await OsuAPI.GetBeatmapAsync(uint.Parse(temp.Substring(2)));
                }
                else
                {
                    return Maplist[num][0] + Environment.NewLine + Maplist[num][1];
                }
                API.SendPhoto(map?.Thumbnail, message.Chat, $"{Maplist[num][1]}{Environment.NewLine}{Maplist[num][0]} {ModBeatmap.FormatMapInfo(map, string.Empty)}", ParseMode.Html);
                return string.Empty;
            }
            else
            {
                Log.Info(this, "Trying to start again");
                if (!Start())
                    return string.Empty;
            }
            return string.Empty;
        }
    }
}
