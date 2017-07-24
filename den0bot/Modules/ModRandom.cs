using System;
using System.Collections.Generic;

namespace den0bot.Modules
{
    class ModRandom : IModule
    {
        private Random rng;

        private List<int> usedMemes;
        private readonly List<string> memeBase = Config.memes;

        public ModRandom()
        {
            Log.Info(this, "Enabled");
            rng = new Random();

            usedMemes = new List<int>();
        }

        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            if (msg.StartsWith("shitposter"))
                return GetRandomShitposter();
            else if (msg.StartsWith("den0saur"))
                return GetRandomDinosaur(sender);
            else if (msg.StartsWith("roll"))
                return Roll(msg);
            else if (msg.StartsWith("meme"))
                return GetRandomMeme(sender);
            else
                return string.Empty;
        }

        public override void Think(){}

        private string GetRandomShitposter()
        {
            return Extensions.GetUsername((Users)rng.Next(0, (int)Users.UserCount)) + " - щитпостер"; 
        }

        private string GetRandomDinosaur(Telegram.Bot.Types.Chat sender)
        {
            switch (rng.Next(1, 4))
            {
                case 1: return "динозавр?";
                case 2:
                    {
                        API.api.SendStickerAsync(sender.Id, new Telegram.Bot.Types.FileToSend("BQADAgADNAADnML7Dbv6HgazQYiIAg"));
                        return string.Empty;
                    }
                case 3:
                    {
                        API.api.SendStickerAsync(sender.Id, new Telegram.Bot.Types.FileToSend("BQADAgADMAADnML7DXy6fUB4x-sqAg"));
                        return string.Empty;
                    }
                default: return string.Empty;
            }
        }

        private string Roll(string msg)
        {
            try
            {
                int i = rng.Next(1, int.Parse(msg.Remove(0, 5))+1); // random is (minvalue, maxvalue-1)
                return "Нароллил " + i;
            }
            catch (Exception e)
            {
                if (e is OverflowException)
                    return "Нихуя ты загнул";

                return "Нароллил " + rng.Next(1, 101).ToString();
            }

        }

        private string GetRandomMeme(Telegram.Bot.Types.Chat sender)
        {
            if (usedMemes.Count == memeBase.Count)
                usedMemes.Clear();

            int num = rng.Next(0, memeBase.Count+1);
            if (usedMemes.Find(x => x == num) != 0)
                return GetRandomMeme(sender);

            string photo = memeBase[num];
            API.SendPhoto(photo, sender);

            usedMemes.Add(num);

            return string.Empty;
        }
    }
}
