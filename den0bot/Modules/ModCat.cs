// den0bot (c) StanR 2019 - MIT License
using System;
using System.Linq;
using Telegram.Bot.Types;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using den0bot.Util;

namespace den0bot.Modules
{
	internal class ModCat : IModule, IReceiveAllMessages
	{
		private readonly string api_link = "https://api.thecatapi.com/v1/images/search?size=med&type=jpg,png&api_key=" + Config.Params.CatToken;

		private readonly Dictionary<long, DateTime> nextPost = new Dictionary<long, DateTime>(); // chatID, time
		private const int cooldown = 5; // minutes

		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(Config.Params.CatToken))
				return;

			if (!string.IsNullOrEmpty(message.Text))
			{
				if (!nextPost.ContainsKey(message.Chat.Id))
					nextPost.Add(message.Chat.Id, DateTime.Now);

				if (nextPost[message.Chat.Id] < DateTime.Now)
				{
					var trigger = Localization.Get("cat_trigger", message.Chat.Id);
					string cat = message.Text.ToLower()
						.Split(' ')
						.FirstOrDefault(x =>
							x.Contains(trigger) && !x.Contains("котор")) // fixme: add exceptions to database as well?
						?.Replace(trigger, trigger.ToUpperInvariant());

					if (cat != null)
					{
						string json = string.Empty;
						try
						{
							json = await Web.DownloadString(api_link);
						}
						catch (Exception)
						{
							return;
						}

						if (!string.IsNullOrEmpty(json))
						{
							JArray obj = JArray.Parse(json);
							await API.SendPhoto(obj[0]["url"].ToString(), message.Chat.Id,
								string.Format(Localization.Get("cat_reply", message.Chat.Id), cat));
						}
						else
						{
							await API.SendMessage(Localization.Get("cat_fail", message.Chat.Id), message.Chat.Id);
						}

						nextPost[message.Chat.Id] = DateTime.Now.AddMinutes(cooldown);
					}
				}
			}
		}
	}
}
