// den0bot (c) StanR 2021 - MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("User")]
	public class User
	{
		[Key]
		public long TelegramID { get; set; }
		public string Username { get; set; }
	}
}
