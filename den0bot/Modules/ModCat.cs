﻿using System;
using System.Linq;
using System.Xml;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModCat : IModule
    {
        public override void Think() { }
        public override bool NeedsAllMessages => true;

        private readonly string api_link = "http://thecatapi.com/api/images/get?format=xml&size=small&type=jpg,png&api_key=" + Config.cat_token;

        private DateTime nextPost = DateTime.Now;

        public override string ProcessCommand(string msg, Chat sender)
        {
            if (nextPost < DateTime.Now)
            {
                string cat = msg.ToLower()
                    .Split(' ')
                    .Where(x => (x.Contains("кот") && !x.Contains("котор")))
                    .FirstOrDefault()
                    ?.Replace("кот", "КОТ");

                if (cat != null)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(api_link);

                    API.SendPhoto(xml.SelectSingleNode("response/data/images/image/url")?.InnerText, sender, string.Format("Кто-то сказал {0}?", cat));

                    nextPost = DateTime.Now.AddMinutes(5);
                }
            }
            return string.Empty;
        }

    }
}
