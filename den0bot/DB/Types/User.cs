﻿// den0bot (c) StanR 2020 - MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("User")]
	public class User
	{
		[Key]
		public int TelegramID { get; set; }
		public string Username { get; set; }
	}
}
