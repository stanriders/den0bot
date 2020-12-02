// den0bot (c) StanR 2020 - MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Santa")]
	public class Santa
	{
		[Key]
		public string Sender { get; set; }

		public string Receiver { get; set; }
	}
}
