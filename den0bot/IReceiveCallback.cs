// den0bot (c) StanR 2018 - MIT License
namespace den0bot
{
	interface IReceiveCallback
	{
		void ReceiveCallback(Telegram.Bot.Types.CallbackQuery callback);
	}
}
