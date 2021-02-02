// den0bot (c) StanR 2021 - MIT License
namespace den0bot.Types
{
	public class ImageCommandAnswer : ICommandAnswer
	{
		public string Image { get; set; }
		public string Caption { get; set; }
		public bool SendTextIfFailed { get; set; }
	}
}
