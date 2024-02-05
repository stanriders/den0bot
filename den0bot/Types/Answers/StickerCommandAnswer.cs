// den0bot (c) StanR 2024 - MIT License

using Telegram.Bot.Types;

namespace den0bot.Types.Answers
{
	public class StickerCommandAnswer : ICommandAnswer
	{
		public InputFileId Sticker { get; set; }

		public StickerCommandAnswer(string stickerId)
		{
			Sticker = new InputFileId(stickerId);
		}
	}
}
