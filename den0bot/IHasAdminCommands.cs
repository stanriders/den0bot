// den0bot (c) StanR 2017 - MIT License
namespace den0bot
{
    interface IHasAdminCommands
    {
        string ProcessAdminCommand(Telegram.Bot.Types.Message message);
    }
}
