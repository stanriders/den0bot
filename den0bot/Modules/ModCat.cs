// den0bot (c) StanR 2018 - MIT License
using System;
using System.Net;
using System.Linq;
using Telegram.Bot.Types;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace den0bot.Modules
{
	class ModCat : IModule, IReceiveAllMessages
	{
		private readonly string api_link = "https://api.thecatapi.com/v1/images/search?size=med&type=jpg,png&api_key=" + Config.cat_token;

		private Dictionary<long, DateTime> nextPost = new Dictionary<long, DateTime>(); // chatID, time
		private readonly int cooldown = 5; // minutes

		public void ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(Config.cat_token))
				return;

			if (!nextPost.ContainsKey(message.Chat.Id))
				nextPost.Add(message.Chat.Id, DateTime.Now);

			if (nextPost[message.Chat.Id] < DateTime.Now)
			{
				var trigger = Localization.Get("cat_trigger", message.Chat.Id);
				string cat = message.Text.ToLower()
					.Split(' ')
					.Where(x => (x.Contains(trigger) && !x.Contains("котор"))) // fixme: add exceptions to database as well?
					.FirstOrDefault()
					?.Replace(trigger, trigger.ToUpperInvariant());

				if (cat != null)
				{
					string json = string.Empty;
					try
					{
						json = new WebClient().DownloadString(api_link);
					}
					catch (Exception) { }

					if (!string.IsNullOrEmpty(json))
					{
						JArray obj = JArray.Parse(json);
						API.SendPhoto(obj[0]["url"].ToString(), message.Chat, string.Format(Localization.Get("cat_reply", message.Chat.Id), cat));
					}
					else
						API.SendMessage(Localization.Get("cat_fail", message.Chat.Id), message.Chat);

					nextPost[message.Chat.Id] = DateTime.Now.AddMinutes(cooldown);
				}
			}
		}
	}
}
