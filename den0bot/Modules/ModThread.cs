// den0bot (c) StanR 2018 - MIT License
#define ENDLESSTHREAD

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using den0bot.Util;

namespace den0bot.Modules
{
    class ModThread : IModule
    {
        private readonly int default_post_amount = 1;

        public ModThread()
        {
            AddCommand(new Command()
            {
                Name = "thread",
                ActionAsync = (msg) =>
                {
                    try
                    {
                        int amount = int.Parse(msg.Text.Remove(0, 7));
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
            });
            Log.Info(this, "Enabled");
        }

        public int FindThread()
        {
#if ENDLESSTHREAD
            return 4011800;
#else
            try
            {
                var data = new WebClient().DownloadData("https://2ch.hk/a/catalog_num.json");
                JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

                foreach (JObject o in obj["threads"])
                {
                    string subject = o["subject"].ToString().ToLower();
                    if (subject == ("osu!thread"))
                    {
                        Log.Info(this, subject + " | " + o["num"].ToString());
                        return o["num"].Value<int>();
                    }

                }
            }
            catch (Exception ex) { Log.Error(this, ex.InnerMessageIfAny()); }

            return 0;
#endif
        }

        public async Task<string> GetLastPosts(int amount)
        {
            int threadID = FindThread();
            if (threadID == 0)
                return "А треда-то нет!";

            try
            {
                string request = $"https://2ch.hk/a/res/{threadID}.json";
                var data = await new WebClient().DownloadDataTaskAsync(request);

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
