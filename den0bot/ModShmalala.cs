//using Neural.NET;

namespace den0bot
{
    class ModShmalala : IModule
    {
        public override bool NeedsAllMessages() => true;

        public ModShmalala()
        {
        }
        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            return string.Empty;
        }

        public override void Think()
        {
        }
    }
}
