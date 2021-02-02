// den0bot (c) StanR 2021 - MIT License

using System.Threading.Tasks;

namespace den0bot.Types
{
	public interface IReceiveCallback
	{
		Task<string> ReceiveCallback(Telegram.Bot.Types.CallbackQuery callback);
	}
}
