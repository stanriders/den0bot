// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModMaplist : IModule
    {
        public override void Think() { }

        private List<string[]> Maplist = new List<string[]>();
        private Random rng = new Random();
        private readonly string spreadsheet = "1AxoXTpNjFnWsFPuSa8rlMbtSnMtkYnKZyzUY_4FTbig";

        private bool isEnabled = false;

        public ModMaplist()
        {
            Start();
            Log.Info(this, $"Enabled, {Maplist.Count} maps");
        }

        private void Start()
        {
            Log.Info(this, "Loading...");
            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://sheets.googleapis.com/v4/spreadsheets/" + spreadsheet + "/values/A2:B999?key=" + Config.googleapi_token;
                    var data = web.DownloadData(request);
                    web.Dispose();

                    JArray array = JToken.Parse(Encoding.UTF8.GetString(data))["values"] as JArray;

                    foreach (JToken token in array)
                    {
                        Maplist.Add(new string[] { token[0].ToString(), token[1].ToString() });
                    }

                    isEnabled = true;
                }
            }
            catch (Exception ex) { Log.Error(this, "Failed to start: " + ex.InnerMessageIfAny()); }
        }

        public override string ProcessCommand(string msg, Chat sender)
        {
            if (msg.StartsWith("map"))
            {
                if (isEnabled)
                {
                    int num = rng.Next(Maplist.Count);
                    return Maplist[num][0] + Environment.NewLine + Maplist[num][1];
                }
                else
                {
                    Log.Info(this, "Trying to start again");
                    Start();
                }
            }
            return string.Empty;
        }
    }
}
