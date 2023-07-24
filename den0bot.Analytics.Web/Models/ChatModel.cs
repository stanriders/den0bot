// den0bot (c) StanR 2021 - MIT License
namespace den0bot.Analytics.Web.Models
{
	public class ChatModel
	{
		public class UserTable
		{
			public class User
			{
				public string Name { get; set; }
				public string Username { get; set; }
				public string Avatar { get; set; }
				public long Messages { get; set; }
				public long Commands { get; set; }
				public long Stickers { get; set; }
				public long Voices { get; set; }
				public double AverageLength { get; set; }
				public string LastMessageTime { get; set; }
				public long Id { get; set; }
			}

			public User[] Users { get; set; }
		}

		public UserTable UsersTable { get; set; } = new();

		public long ChatId { get; set; }
	}
}
