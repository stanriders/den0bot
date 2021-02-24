// den0bot (c) StanR 2021 - MIT License
using System.ComponentModel.DataAnnotations;

namespace den0bot.Analytics.Data.Types
{
	public class User
	{
		[Key]
		public long Id { get; set; }
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
