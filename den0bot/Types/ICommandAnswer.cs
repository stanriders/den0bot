// den0bot (c) StanR 2025 - MIT License
using Telegram.Bot.Types.ReplyMarkups;

namespace den0bot.Types
{
	public abstract class ICommandAnswer
	{
		public ReplyMarkup ReplyMarkup { get; set; }
	}
}
