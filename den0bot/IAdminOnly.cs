// den0bot (c) StanR 2017 - MIT License
namespace den0bot
{
    interface IAdminOnly
    {
        string ProcessAdminCommand(string msg, Telegram.Bot.Types.Chat sender);
    }
}
