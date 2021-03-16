// den0bot (c) StanR 2021 - MIT License

using Telegram.Bot.Types.InputFiles;

namespace den0bot.Types.Answers
{
	public class StickerCommandAnswer : ICommandAnswer
	{
		public InputOnlineFile Sticker { get; set; }

		public StickerCommandAnswer(string stickerId)
		{
			Sticker = new InputOnlineFile(stickerId);
		}
	}
}
