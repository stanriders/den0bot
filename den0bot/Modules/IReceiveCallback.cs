// den0bot (c) StanR 2019 - MIT License
namespace den0bot.Modules
{
	public interface IReceiveCallback
	{
		void ReceiveCallback(Telegram.Bot.Types.CallbackQuery callback);
	}
}
