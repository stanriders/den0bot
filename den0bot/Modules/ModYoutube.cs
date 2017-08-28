using System;
using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
    // check osu!content's youtube channel and post new highscores to all chats
    class ModYoutube : IModule
    {
        private DateTime nextCheck;

        private readonly string api_key = Config.googleapi_token;
        private readonly string channel_id = "UC-Slh6DZ_G-_hqmjZFtlusg";  // osu!content
        private readonly double check_interval = 1.0; // minutes
        private readonly int default_score_amount = 3;

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
            {
                try
                {
                    int amount = int.Parse(msg.Remove(0, 10));
                    if (amount > 20)
                        return GetLastScores(default_score_amount);
                    else
                        return GetLastScores(amount);
                }
                catch (Exception)
                {
                    return GetLastScores(default_score_amount);
                }
            }
            return string.Empty;
        }

        private async void Update(DateTime lastChecked)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    lastChecked = lastChecked.AddMinutes(-check_interval);

                    string request = string.Format("https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" +
                                                    "&key={0}"+ "&fields={1}" + "&publishedAfter={2}" + "&channelId={3}", 
                                                    api_key, 
                                                    Uri.EscapeDataString("items(contentDetails/upload,snippet/title)"), 
                                                    Uri.EscapeDataString(lastChecked.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ")), 
                                                    channel_id);


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

        private string GetLastScores(int amount)
        {
            string result = string.Empty;
            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = string.Format("https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" + 
                                                    "&maxResults={0}" + "&key={1}" + "&fields={2}" + "&channelId={3}",
                                                    amount,
                                                    api_key, 
                                                    Uri.EscapeDataString("items(contentDetails/upload,snippet/title)"), 
                                                    channel_id);

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
