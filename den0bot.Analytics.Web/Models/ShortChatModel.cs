// den0bot (c) StanR 2024 - MIT License
using System;

namespace den0bot.Analytics.Web.Models
{
	public class ShortChatModel
	{
		public string? Name { get; set; }
		public string? Avatar { get; set; }
		public long Messages { get; set; }
        public long Id { get; set; }
		public DateTime LastMessageTimestamp { get; set; }
	}
}
