// den0bot (c) StanR 2020 - MIT License
#define ENDLESSTHREAD

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Util;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	class ModThread : IModule
	{
		private class Board
		{
			public class Thread
			{
				public class Post
				{
					public class File
					{
						public string displayname { get; set; }
						public string path { get; set; }
					}
					public List<File> files { get; set; }
					public int num { get; set; }
					public string name { get; set; }
					public string comment { get; set; }
				}
				public List<Post> posts { get; set; }
			}
			public List<Thread> threads { get; set; }
		}

		private const int max_post_amount = 15;
		private const int default_post_amount = 1;

		public ModThread()
		{
			AddCommand(new Command()
			{
				Name = "thread",
				ParseMode = ParseMode.Html,
				ActionAsync = (msg) =>
				{
					try
					{
						int amount = int.Parse(msg.Text.Remove(0, 7));
						if (amount > max_post_amount)
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
			Log.Debug("Enabled");
		}

		private int FindThread()
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

		private string FilterPost(string value)
		{
			var step1 = value.Replace("/a/res/", "http://2ch.hk/a/res/")
				.Replace("<br>", "\n")
				.Replace(@"</span>", "</code>");

			return Regex.Replace(step1, @"<span[^>]+>", "<code>")
				.Replace(@"\s{2,}", " ")
				.Replace("&gt;", ">")
				.Replace("&nbsp;", " ")
				.Replace("&quot;", "\"")
				.Replace("&#47;", "/");
		}

		private async Task<string> GetLastPosts(int amount)
		{
			int threadID = FindThread();
			if (threadID == 0)
				return "А треда-то нет!";

			try
			{
				string request = $"https://2ch.hk/a/res/{threadID}.json";
				var data = await Web.DownloadString(request);
				Board thread = JsonConvert.DeserializeObject<Board>(data);

				var posts = thread.threads[0].posts;
				if (posts.Count > amount)
					posts.RemoveRange(0, posts.Count - amount);

				string result = string.Empty;
				foreach (var post in posts)
				{
					var images = string.Empty;
					foreach (var file in post.files)
						images += $"[<a href=\"https://2ch.hk{file.path}\">{file.displayname}</a>]";

					result += $"<code>{post.name}({post.num})</code> {images}\n" +
					          $"{FilterPost(post.comment)}\n" +
					          $"_______\n";
				}

				return result + $"[<a href=\"https://2ch.hk/a/res/{threadID}.html\">Thread</a>]";
			}
			catch (Exception ex) { Log.Error(ex.InnerMessageIfAny()); }

			return string.Empty;
		}
	}
}
