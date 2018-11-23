using SQLite;

namespace den0bot.DB.Types
{
	class Santa
	{
		[PrimaryKey]
		public string Sender { get; set; }

		public string Receiver { get; set; }
	}
}
