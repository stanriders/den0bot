using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
    class ModThread : IModule
    {
        private readonly int default_post_amount = 0;

        public ModThread()
        {
            Log.Info(this, "Enabled");
        }

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            if (msg.StartsWith("thread"))
            {
                try
                {
                    int amount = int.Parse(msg.Remove(0, 7));
                    if (amount > 20)
                        return GetLastFivePosts(default_post_amount);
                    else
                        return GetLastFivePosts(amount);
                }
                catch (Exception)
                {
                    return GetLastFivePosts(default_post_amount);
                }
            }
            else
                return string.Empty;
        }

        public override void Think() { }

        public int FindThread()
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    var data = web.DownloadData("https://2ch.hk/a/catalog_num.json");

                    JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

                    foreach (JObject o in obj["threads"])
                    {
                        string subject = (string)o["subject"];
                        if (subject.ToLower().StartsWith("osu") || subject.ToLower().StartsWith("осу"))
                        { 
                            Log.Info(this, subject + " | " + (string)o["num"]);
                            return (int)o["num"];
                        }

                    }
                }
            }
            catch (Exception ex) { Log.Error(this, ex.Message); }

            return 0;
        }

        public string GetLastFivePosts(int numofPosts)
        {
            string result = string.Empty;

            int threadID = FindThread();

            if (threadID == 0)
                return "А треда-то нет!";

            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://2ch.hk/a/res/" + threadID + ".json";
                    var data = web.DownloadData(request);

                    JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

                    result += "https://2ch.hk/a/res/" + threadID + ".html" + " | Постов: " + obj["posts_count"] + Environment.NewLine;

                    List<string> list = new List<string>();
                    
                    foreach (JObject o in obj["threads"][0]["posts"])
                    {
                        string msg = (string)o["comment"];

                        if (o["files"].HasValues)
                            msg = "http://2ch.hk" + (string)o["files"][0]["path"] + "\n" + (string)o["comment"];

                        list.Add(msg);
                    }

                    if (list.Count > numofPosts)
                        list.RemoveRange(0, list.Count - numofPosts);

                    for (int i = 0; i < list.Count; i++)
                    {
                        result += "___________" + Environment.NewLine;

                        result += Extensions.FilterHTML(list[i]) + Environment.NewLine;
                    }
                }
            }
            catch (Exception ex) { Log.Error(this, ex.Message); }

            return result;
        }
    }
}
