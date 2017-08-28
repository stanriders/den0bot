using Telegram.Bot.Types.Enums;

namespace den0bot
{
    abstract class IModule
    {
        public virtual bool NeedsAllMessages => false;
        public virtual bool NeedsPhotos => false;
        public virtual ParseMode ParseMode => ParseMode.Default;

        public abstract void Think();
        public abstract string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender);
    }
}
