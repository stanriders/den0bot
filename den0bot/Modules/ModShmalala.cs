// den0bot (c) StanR 2017 - MIT License
//using Neural.NET;

namespace den0bot.Modules
{
    class ModShmalala : IModule
    {
        public override bool NeedsAllMessages => true;

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
