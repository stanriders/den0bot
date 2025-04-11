using System;
using System.Linq;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	public class ModBegemotikIdiNahui(ILogger<IModule> logger) : IModule(logger), IReceiveAllMessages, IReceiveForwards
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

		private const string begemotik_idi_nahuy =
			"CAACAgIAAx0EUctp0AACIypn-QsKtcEC7dtCbYl26fn_48jvFgACFGYAAnewwUqltednG_eKXzYE";

		private const string begemotik_idi_nahuy_idi_nahuy =
			"CAACAgIAAx0CUctp0AACIy9n-QtAdBOUPD15KAFdeD1gYqOXagAC-3QAAnFkwUrqtAGrfploezYE";

		private const string begemotik_idi_nahuy_idi_nahuy_idi_nahuy =
			"CAACAgIAAxkBAAEOSBNn-QiTND5o09gWAVsR6woV0NLdCgACxV8AAkaUwEoNGnCBIHKe6DYE";

		private const long ceo_of_life = -1001269393237;

		public async Task ReceiveMessage(Message message)
		{
			if (message.Sticker?.FileId == begemotik_idi_nahuy_idi_nahuy &&
			    message.ReplyToMessage?.Sticker?.FileId == begemotik_idi_nahuy &&
			    message.ReplyToMessage?.From?.Id == API.BotUser.Id)
			{
				await API.SendSticker(
					new InputFileId(begemotik_idi_nahuy_idi_nahuy_idi_nahuy),
					message.Chat.Id, message.MessageId);

				return;
			}

			var sticker = message.Sticker ?? message.ReplyToMessage?.Sticker;

			if (sticker?.SetName != null)
			{
				if (knownPacks.Contains(sticker.SetName))
				{
					await API.SendSticker(
						new InputFileId(begemotik_idi_nahuy),
						message.Chat.Id, message.MessageId);

					return;
				}

				if (sticker.SetName.Contains("hippo", StringComparison.InvariantCultureIgnoreCase) &&
				    sticker.SetName.Contains("nyasticks", StringComparison.InvariantCultureIgnoreCase))
				{
					await API.SendSticker(
						new InputFileId(begemotik_idi_nahuy),
						message.Chat.Id, message.MessageId);

					return;
				}
			}

			if (message.ForwardFromChat?.Id == ceo_of_life)
			{
				await API.SendSticker(
					new InputFileId("CAACAgIAAxkBAAEOSCdn-QspIzkRlLL81Ucr6xS5093tRwACs2UAAvXjwEqlrXpLqGMgHTYE"),
					message.Chat.Id, message.MessageId);

				return;
			}
		}
	}
}
