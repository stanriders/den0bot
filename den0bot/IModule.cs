
namespace den0bot
{
    abstract class IModule
    {
        public virtual bool NeedsAllMessages() => false;
        abstract public void Think();
        abstract public string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender);
    }
}
