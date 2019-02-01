// den0bot (c) StanR 2019 - MIT License
namespace den0bot.Modules
{
	interface IReceiveCallback
	{
		void ReceiveCallback(Telegram.Bot.Types.CallbackQuery callback);
	}
}
