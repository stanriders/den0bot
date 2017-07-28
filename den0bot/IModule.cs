using Telegram.Bot.Types.Enums;

namespace den0bot
{
    abstract class IModule
    {
        public virtual bool NeedsAllMessages() => false;
        public virtual ParseMode ParseMode => ParseMode.Default;

        abstract public void Think();
        abstract public string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender);
    }
}
