// den0bot (c) StanR 2017 - MIT License
using Telegram.Bot.Types.Enums;

namespace den0bot
{
    abstract class IModule
    {
        public virtual bool NeedsAllMessages => false;
        public virtual bool NeedsPhotos => false;
        public virtual ParseMode ParseMode => ParseMode.Default;

        public abstract void Think();
        public abstract string ProcessCommand(Telegram.Bot.Types.Message message);
    }
}
