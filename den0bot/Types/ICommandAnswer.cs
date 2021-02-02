// den0bot (c) StanR 2021 - MIT License
using Telegram.Bot.Types.ReplyMarkups;

namespace den0bot.Types
{
	public abstract class ICommandAnswer
	{
		public IReplyMarkup ReplyMarkup { get; set; }
	}
}
