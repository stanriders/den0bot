// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
    class ModThread : IModule
    {
        private readonly int default_post_amount = 1;

        public ModThread()
        {
            Log.Info(this, "Enabled");
        }

        public override string ProcessCommand(Telegram.Bot.Types.Message message)
        {
            if (message.Text.StartsWith("thread"))
            {
                try
                {
                    int amount = int.Parse(message.Text.Remove(0, 7));
                    if (amount > 20)
                        return GetLastPosts(default_post_amount);
                    else
                        return GetLastPosts(amount);
                }
                catch (Exception)
                {
                    return GetLastPosts(default_post_amount);
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
                WebClient web = new WebClient();
                var data = web.DownloadData("https://2ch.hk/a/catalog_num.json");
                web.Dispose();

                JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

                foreach (JObject o in obj["threads"])
                {
                    string subject = o["subject"].ToString().ToLower();
                    if (subject.StartsWith("osu") || subject.StartsWith("осу"))
                    {
                        Log.Info(this, subject + " | " + o["num"].ToString());
                        return o["num"].Value<int>();
                    }

                }
            }
            catch (Exception ex) { Log.Error(this, ex.InnerMessageIfAny()); }

            return 0;
        }

        public string GetLastPosts(int amount)
        {
            int threadID = FindThread();
            if (threadID == 0)
                return "А треда-то нет!";

            try
            {
                string request = $"https://2ch.hk/a/res/{threadID}.json";

                WebClient web = new WebClient();
                var data = web.DownloadData(request);
                web.Dispose();

                JToken obj = JObject.Parse(Encoding.UTF8.GetString(data))["threads"][0];

                string result = $"https://2ch.hk/a/res/{threadID}.html{Environment.NewLine}";

                List<string> posts = new List<string>();

                foreach (JToken o in obj["posts"])
                {
                    string msg = o["comment"].ToString();

                    if (o["files"].HasValues)
                        msg = "http://2ch.hk" + o["files"][0]["path"].ToString() + Environment.NewLine + o["comment"].ToString();

                    posts.Add(msg);
                }

                if (posts.Count > amount)
                    posts.RemoveRange(0, posts.Count - amount);

                foreach (string post in posts)
                {
                    result += "___________" + Environment.NewLine + post.FilterHTML() + Environment.NewLine;
                }

                return result;
            }
            catch (Exception ex) { Log.Error(this, ex.InnerMessageIfAny()); }

            return string.Empty;
        }
    }
}
