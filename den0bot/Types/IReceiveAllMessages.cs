// den0bot (c) StanR 2021 - MIT License

using System.Threading.Tasks;

namespace den0bot.Types
{
	public interface IReceiveAllMessages
	{
		Task ReceiveMessage(Telegram.Bot.Types.Message message);
	}
}
