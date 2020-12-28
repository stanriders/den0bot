// den0bot (c) StanR 2020 - MIT License
using System.Threading.Tasks;
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

		public async Task ReceiveMessage(Message message)
		{
			await API.SendMessage("hi", message.Chat.Id);
		}

		private string Go(Message msg)
		{
			return "ok!";
		}
	}
}
