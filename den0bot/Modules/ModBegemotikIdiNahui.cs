using System;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	public class ModBegemotikIdiNahui(ILogger<IModule> logger) : IModule(logger), IReceiveAllMessages
	{
		public async Task ReceiveMessage(Message message)
		{
			if (message.Sticker?.SetName != null && 
				message.Sticker.SetName.Contains("бегемо", StringComparison.InvariantCultureIgnoreCase) && 
				message.Sticker.SetName.Contains("nyasticks", StringComparison.InvariantCultureIgnoreCase))
			{
				await API.SendSticker(
					new InputFileId("CAACAgIAAxkBAAEOQsRn9Si9wV_SU5o1IAVLjOPQxiFPkgACFGYAAnewwUqltednG_eKXzYE"),
					message.Chat.Id, message.MessageId);
			}
		}
	}
}
