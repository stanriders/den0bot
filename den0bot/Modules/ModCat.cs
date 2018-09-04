// den0bot (c) StanR 2017 - MIT License
using System;
using System.Linq;
using System.Xml;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModCat : IModule, IProcessAllMessages
    {
        private readonly string api_link = "http://thecatapi.com/api/images/get?format=xml&size=med&type=jpg,png&api_key=" + Config.cat_token;

        private DateTime nextPost = DateTime.Now;

        public void ReceiveMessage(Message message)
        {
            if (nextPost < DateTime.Now)
            {
                string cat = message.Text.ToLower()
                    .Split(' ')
                    .Where(x => (x.Contains("кот") && !x.Contains("котор")))
                    .FirstOrDefault()
                    ?.Replace("кот", "КОТ");

                if (cat != null)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(api_link);

                    string res = xml.SelectSingleNode("response/data/images/image/url")?.InnerText;

                    if (!string.IsNullOrEmpty(res))
                        API.SendPhoto(res, message.Chat, string.Format("Кто-то сказал {0}?", cat));
                    else
                        API.SendMessage("КОТа сегодня не будет...", message.Chat);

                    nextPost = DateTime.Now.AddMinutes(5);
                }
            }
        }
    }
}
