namespace den0bot
{
    interface IAdminOnly
    {
        string ProcessAdminCommand(string msg, Telegram.Bot.Types.Chat sender);
    }
}
