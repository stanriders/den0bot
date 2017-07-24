using System;
using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
    // check CirclePeople's youtube channel and post new highscores to all chats
    class ModYoutube : IModule
    {
        private DateTime nextCheck;

        private readonly string api_key = Config.youtube_token;
        private readonly string channel_id = "UC-Slh6DZ_G-_hqmjZFtlusg";  // osu!content
        private readonly double check_interval = 1.0; // minutes

        public ModYoutube()
        {
            nextCheck = DateTime.Now;

            Log.Info(this, "Enabled");
        }

        public override void Think()
        {
            if (nextCheck < DateTime.Now)
            {
                Update(nextCheck);
                nextCheck = DateTime.Now.AddMinutes(check_interval);
            }
        }

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            if (msg.StartsWith("newscores"))
                return GetLastThreeScores();
            else
                return string.Empty;
        }

        private async void Update(DateTime lastChecked)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    lastChecked = lastChecked.AddMinutes(-check_interval);

                    string request = "https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails&key=" + api_key + "&fields=" +
                                                                Uri.EscapeDataString("items(contentDetails/upload,snippet/title)") + 
                                                                "&publishedAfter=" + Uri.EscapeDataString(lastChecked.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ")) +
                                                                "&channelId=" + channel_id;

                    var data = await web.DownloadDataTaskAsync(request);

                    JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

                    if (obj["items"].HasValues)
                    {
                        foreach (JObject vid in obj["items"])
                        {
                            string title = (string)vid["snippet"]["title"];
                            string id = (string)vid["contentDetails"]["upload"]["videoId"];

                            string result = "❗️ " + title + "\n" + "http://youtu.be/" + id;
                            API.SendMessageToAllChats(result);
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Error(this, ex.Message); }
        }

        private string GetLastThreeScores()
        {
            string result = string.Empty;
            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails&maxResults=3&key=" + api_key + "&fields=" +
                                                                Uri.EscapeDataString("items(contentDetails/upload,snippet/title)") +
                                                                "&channelId=" + channel_id;

                    var data = web.DownloadData(request);

                    JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));
                    for (int i = 0; i < 3; i++)
                    {
                        string title = (string) obj["items"][i]["snippet"]["title"];
                        string id = (string) obj["items"][i]["contentDetails"]["upload"]["videoId"];
                        result += title + "\n" + "http://youtu.be/" + id + Environment.NewLine + Environment.NewLine;
                    }
                }
            }
            catch (Exception ex) { Log.Error(this, ex.Message); }

            return result;
        }
    }
}
