// den0bot (c) StanR 2017 - MIT License
using System;
using Telegram.Bot.Types;
using den0bot.DB;
using System.Linq;
using System.Collections.Generic;

namespace den0bot.Modules
{
    class ModRandom : IModule
    {
        public ModRandom()
        {
            AddCommands(new Command[]
            {
                new Command
                {
                    Name = "roll",
                    Reply = true,
                    Action = (msg) => Roll(msg.Text)
                },
                new Command
                {
                    Name = "meme",
                    Action = (msg) => GetRandomMeme(msg.Chat)
                },
                new Command
                {
                    Name = "shitposter",
                    Action = (msg) => GetRandomShitposter(msg.Chat)
                },
                new Command
                {
                    Name = "den0saur",
                    Action = (msg) => GetRandomDinosaur(msg.Chat)
                }
            });
            Log.Info(this, "Enabled");
        }

        private string GetRandomShitposter(Chat sender)
        {
            if (Database.GetPlayerCount(sender.Id) <= 0)
                return "Ты щитпостер";

            int num = RNG.Next(Database.GetPlayerCount(sender.Id));

            if (Database.GetPlayerChatID(num) != sender.Id)
                return GetRandomShitposter(sender);

            return Database.GetPlayerFriendlyName(num) + " - щитпостер"; 
        }

        private string GetRandomDinosaur(Chat sender)
        {
            switch (RNG.Next(1, 4))
            {
                case 1: return "динозавр?";
                case 2: API.SendSticker(new FileToSend("BQADAgADNAADnML7Dbv6HgazQYiIAg"), sender.Id); break;
                case 3: API.SendSticker(new FileToSend("BQADAgADMAADnML7DXy6fUB4x-sqAg"), sender.Id); break;
            }
            return string.Empty;
        }

        private string Roll(string msg)
        {
            int max = 101;

            List<string> msgArr = msg.Split(' ').ToList();
            if (msgArr.Count > 1)
            {
                try
                {
                    max = (int)uint.Parse(msgArr[1]) + 1;
                }
                catch (Exception e)
                {
                    if (e is OverflowException)
                        return "Нихуя ты загнул";
                }
            }
            return "Нароллил " + RNG.Next(1, max).ToString();
        }

        private string GetRandomMeme(Chat sender)
        {
            int memeCount = Database.GetMemeCount(sender.Id);
            if (memeCount <= 0)
                return "А мемов-то нет";

            string photo = Database.GetMeme(sender.Id);
            if (photo != null && photo != string.Empty)
            { 
                API.SendPhoto(photo, sender);
                return string.Empty;
            }

            return "Чет не получилось";
        }
    }
}
