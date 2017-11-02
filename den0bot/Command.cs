// den0bot (c) StanR 2017 - MIT License
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot
{
    public class Command
    {
        private string name;
        public string Name
        {
            get
            {
                return "/" + name;
            }
            set
            {
                name = value;
            }
        }

        public ParseMode ParseMode;

        public bool IsAdminOnly;
        public bool IsAsync;

        public Func<Message, Task<string>> ActionAsync;
        public Func<Message, string> Action;
    }
}
