using System;
using System.Linq;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	public class ModBegemotikIdiNahui(ILogger<IModule> logger) : IModule(logger), IReceiveAllMessages
	{
		private static string[] knownPacks = new[]
		{
			"Hippo4_0734_Nyasticks",
			"Baby_chubby_Hippo",
			"hippo_cute_bunny_Nyasticks",
			"Paired_Hippo_Nyasticks2",
			"baby_hippo_Nyasticks",
			"hippo3_Nyasticks",
			"Begemotik_2"
		};

		public async Task ReceiveMessage(Message message)
		{
			if (message.Sticker?.SetName != null)
			{
				if (knownPacks.Contains(message.Sticker.SetName))
				{
					await API.SendSticker(
						new InputFileId("CAACAgIAAxkBAAEOQsRn9Si9wV_SU5o1IAVLjOPQxiFPkgACFGYAAnewwUqltednG_eKXzYE"),
						message.Chat.Id, message.MessageId);

					return;
				}

				if (message.Sticker.SetName.Contains("hippo", StringComparison.InvariantCultureIgnoreCase) &&
				    message.Sticker.SetName.Contains("nyasticks", StringComparison.InvariantCultureIgnoreCase))
				{
					await API.SendSticker(
						new InputFileId("CAACAgIAAxkBAAEOQsRn9Si9wV_SU5o1IAVLjOPQxiFPkgACFGYAAnewwUqltednG_eKXzYE"),
						message.Chat.Id, message.MessageId);
				}
			}
		}
	}
}
