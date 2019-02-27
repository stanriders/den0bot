using Telegram.Bot.Types;

namespace den0bot.Modules.Example
{
	public class ModExample : IModule, IReceiveAllMessages
	{
		public ModExample()
		{
			AddCommand(new Command
			{
				Name = "example",
				Action = Go
			});
		}

		public void ReceiveMessage(Message message)
		{
			API.SendMessage("hi", message.Chat);
		}

		private string Go(Message msg)
		{
			return "ok!";
		}
	}
}
