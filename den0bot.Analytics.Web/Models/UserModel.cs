// den0bot (c) StanR 2021 - MIT License
namespace den0bot.Analytics.Web.Models
{
	public class UserModel
	{
		public class ChatTable
		{
			public class Chat
			{
				public string Name { get; set; }
				public string Avatar { get; set; }
				public long Messages { get; set; }
				public long Stickers { get; set; }
				public string LastMessageTime { get; set; }
				public long Id { get; set; }
			}

			public Chat[] Chats { get; set; }
		}

		public ChatTable ChatsTable { get; set; } = new();

		public long UserId { get; set; }
	}
}
