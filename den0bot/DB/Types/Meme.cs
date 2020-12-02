// den0bot (c) StanR 2020 - MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.DB.Types
{
	[Table("Meme")]
	public class Meme
	{
		[Key]
		public int Id { get; set; }

		public string Link { get; set; }

		public long ChatID { get; set; }

		public bool Used { get; set; }
	}
}
