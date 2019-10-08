// den0bot (c) StanR 2019 - MIT License
using System.ComponentModel.DataAnnotations;

namespace den0bot.Analytics.Data.Types
{
	public class Message
	{
		[Key]
		public long Id { get; set; }
		public long TelegramId { get; set; }
		public long Timestamp { get; set; }
		public long UserId { get; set; }
		public long ChatId { get; set; }
		public MessageType Type { get; set; }
		public string Command { get; set; }
		public long Length { get; set; }
	}

	public enum MessageType
	{
		Text = 0,
		Photo = 1,
		Sticker = 2,
		Other = 99
	}
}
