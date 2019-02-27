// den0bot (c) StanR 2019 - MIT License
namespace den0bot.Modules
{
	public interface IReceiveAllMessages
	{
		void ReceiveMessage(Telegram.Bot.Types.Message message);
	}
}
