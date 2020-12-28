// den0bot (c) StanR 2020 - MIT License
namespace den0bot.Analytics.Web.Models
{
	public class ChatModel
	{
		public class UserTable
		{
			public class User
			{
				public string Name { get; set; }
				public string Avatar { get; set; }
				public long Messages { get; set; }
				public long Commands { get; set; }
				public long Stickers { get; set; }
				public double AverageLength { get; set; }
				public long GirlsRequested { get; set; }
				public long GirlsAdded { get; set; }
				public string LastMessageTime { get; set; }
			}

			public User[] Users { get; set; }
		}

		public UserTable UsersTable { get; set; } = new();

		public long ChatId { get; set; }
	}
}
