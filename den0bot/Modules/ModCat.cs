// den0bot (c) StanR 2017 - MIT License
using System;
using System.Net;
using System.Linq;
using Telegram.Bot.Types;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules
{
	class ModCat : IModule, IProcessAllMessages
	{
		private readonly string api_link = "https://api.thecatapi.com/v1/images/search?size=med&type=jpg,png&api_key=" + Config.cat_token;

		private DateTime nextPost = DateTime.Now;

		public void ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(Config.cat_token))
				return;

			if (nextPost < DateTime.Now)
			{
				string cat = message.Text.ToLower()
					.Split(' ')
					.Where(x => (x.Contains("кот") && !x.Contains("котор")))
					.FirstOrDefault()
					?.Replace("кот", "КОТ");

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
						API.SendPhoto(obj[0]["url"].ToString(), message.Chat, string.Format("Кто-то сказал {0}?", cat));
					}
					else
						API.SendMessage("КОТа сегодня не будет...", message.Chat);

                    nextPost = DateTime.Now.AddMinutes(5);
                }
            }
        }
    }
}
