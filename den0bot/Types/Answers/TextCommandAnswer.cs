// den0bot (c) StanR 2021 - MIT License
namespace den0bot.Types.Answers
{
	public class TextCommandAnswer : ICommandAnswer
	{
		public string Text { get; set; }

		public TextCommandAnswer(string text)
		{
			Text = text;
		}
	}
}
