// den0bot (c) StanR 2018 - MIT License
namespace den0bot
{
	interface IReceiveAllMessages
	{
		void ReceiveMessage(Telegram.Bot.Types.Message message);
	}
}
