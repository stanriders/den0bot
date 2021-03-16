// den0bot (c) StanR 2021 - MIT License

using System.Collections.Generic;
using Telegram.Bot.Types;

namespace den0bot.Types.Answers
{
	public class ImageAlbumCommandAnswer : ICommandAnswer
	{
		public List<InputMediaPhoto> Images { get; set; }

		public ImageAlbumCommandAnswer(List<InputMediaPhoto> images)
		{
			Images = images;
		}
	}
}
