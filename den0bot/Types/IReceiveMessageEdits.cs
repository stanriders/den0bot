// den0bot (c) StanR 2021 - MIT License

using System.Threading.Tasks;

namespace den0bot.Types
{
	interface IReceiveMessageEdits
	{
		Task ReceiveMessageEdit(Telegram.Bot.Types.Message message);
	}
}
