﻿// den0bot (c) StanR 2023 - MIT License
#define ENDLESSTHREAD

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Types;
using den0bot.Types.Answers;
using den0bot.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	internal class ModThread : IModule
	{
		private readonly ILogger<IModule> logger;

		private class Board
		{
			public class Thread
			{
				public class Post
				{
					public class File
					{
						[JsonProperty("displayname")]
						public string DisplayName { get; set; }

						[JsonProperty("path")]
						public string Path { get; set; }
					}

					[JsonProperty("files")]
					public List<File>? Files { get; set; }

					[JsonProperty("num")]
					public long Num { get; set; }

					[JsonProperty("name")]
					public string Name { get; set; }

					[JsonProperty("comment")]
					public string Comment { get; set; }
				}
				[JsonProperty("posts")]
				public List<Post> Posts { get; set; }
			}
			[JsonProperty("threads")]
			public List<Thread> Threads { get; set; }
		}

		private const int max_post_amount = 15;
		private const int default_post_amount = 1;

		public ModThread(ILogger<IModule> logger) : base(logger)
		{
			this.logger = logger;
			AddCommand(new Command
			{
				Name = "thread",
				ParseMode = ParseMode.Html,
				ActionAsync = async (msg) =>
				{
					try
					{
						int amount = int.Parse(msg.Text.Remove(0, 7));
						if (amount > max_post_amount)
							return new TextCommandAnswer(await GetLastPosts(default_post_amount));
						else
							return new TextCommandAnswer(await GetLastPosts(amount));
					}
					catch (Exception)
					{
						return new TextCommandAnswer(await GetLastPosts(default_post_amount));
					}
				}
			});
		}

#if !ENDLESSTHREAD
		private int FindThread()
		{
			try
			{
				var data = new WebClient().DownloadData("https://2ch.hk/a/catalog_num.json");
				JObject obj = JObject.Parse(Encoding.UTF8.GetString(data));

				foreach (JObject o in obj["threads"])
				{
					string subject = o["subject"].ToString().ToLower();
					if (subject == ("osu!thread"))
					{
						logger.LogInformation(this, subject + " | " + o["num"].ToString());
						return o["num"].Value<int>();
					}

				}
			}
			catch (Exception ex) { logger.LogError(this, ex.InnerMessageIfAny()); }

			return 0;
		}
#endif

		private static string FilterPost(string value)
		{
			var step1 = value.Replace("/a/res/", "https://2ch.hk/a/res/")
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
#if !ENDLESSTHREAD
			int threadID = FindThread();
			if (threadID == 0)
				return "А треда-то нет!";
#else
			const int threadID = 4011800;
#endif

			try
			{
				string request = $"https://2ch.hk/a/res/{threadID}.json";
				var data = await Web.DownloadString(request);
				Board thread = JsonConvert.DeserializeObject<Board>(data);
				if (thread == null || thread.Threads.Count <= 0 || thread.Threads[0].Posts.Count <= 0)
				{
					logger.LogError("thread == null || thread.Threads.Count <= 0 || thread.Threads[0].Posts.Count <= 0");

					return "Тред не нашелся";
				}
				var posts = thread.Threads[0].Posts;
				if (posts.Count > amount)
					posts.RemoveRange(0, posts.Count - amount);

				string result = string.Empty;
				foreach (var post in posts)
				{
					var images = string.Empty;
					if (post.Files?.Count > 0)
						foreach (var file in post.Files)
							images += $"[<a href=\"https://2ch.hk{file.Path}\">{file.DisplayName}</a>]";

					result += $"<code>{post.Name}({post.Num})</code> {images}\n" +
					          $"{FilterPost(post.Comment)}\n" +
					          $"________\n";
				}

				return result + $"[<a href=\"https://2ch.hk/a/res/{threadID}.html\">Thread</a>]";
			}
			catch (Exception ex) { logger.LogError(ex, ex.InnerMessageIfAny()); }

			return string.Empty;
		}
	}
}
