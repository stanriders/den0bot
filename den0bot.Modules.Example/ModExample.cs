// den0bot (c) StanR 2021 - MIT License
using den0bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace den0bot.Modules.Example
{
	public class ModExample : IModule, IReceiveAllMessages
	{
		public ModExample()
		{
			AddCommands(new[] 
			{
				new Command
				{
					Name = "example",
					Action = Go
				},
				new Command
				{
					Name = "exampleasync",
					ActionAsync = GoAsync,
					Reply = true
				}
			});
		}

		public async Task ReceiveMessage(Message message)
		{
			_ = await API.SendMessage("hi", message.Chat.Id);
		}

		private ICommandAnswer Go(Message msg)
		{
			return new TextCommandAnswer("ok!");
		}

		private async Task<ICommandAnswer> GoAsync(Message msg)
		{
			await Task.Delay(1000);
			return new TextCommandAnswer("ok but 1 second later!");
		}
	}
}
