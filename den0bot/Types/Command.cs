// den0bot (c) StanR 2021 - MIT License
using den0bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Types.Answers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot.Types
{
	public class Command
	{
		public List<string> Names { get; } = new();
		public string Name
		{
			set => Names.Add(value);
		}
		public bool ContainsName(string name)
		{
			return Names.Contains(name[1..]);
		}

		/// <summary>
		/// <see cref="Telegram.Bot.Types.Enums.ParseMode" />
		/// </summary>
		public ParseMode ParseMode { get; init; }

		/// <summary>
		/// Determines if command can only be used by chat admins
		/// </summary>
		public bool IsAdminOnly { get; init; }

		/// <summary>
		/// Determines if command can only be used by bot owner
		/// </summary>
		public bool IsOwnerOnly { get; init; }

		/// <summary>
		/// Determines if command result should be a reply to the command message
		/// </summary>
		public bool Reply { get; init; }

		public Func<Message, Task<ICommandAnswer>> ActionAsync { get; init; }
		public Func<Message, ICommandAnswer> Action { get; init; }

		/// <summary>
		/// Function to call AFTER action is complete and sent
		/// </summary>
		public Action<Message> ActionResult { get; init; }

		private async Task<bool> IsAllowedAsync(Message message)
		{
			var from = message.From.Username;

			// owner can run any commands
			if (from == Config.Params.OwnerUsername)
				return true;

			if (IsOwnerOnly)
				return from == Config.Params.OwnerUsername;

			if (IsAdminOnly)
				return (await API.GetAdmins(message.Chat.Id)).Any(x => x.User.Username == from && (x.CanPromoteMembers ?? true));

			return true;
		}

		// FIXME: this really shouldnt be in the command class
		public async Task<bool> Run(Message message)
		{
			var senderChatId = message.Chat.Id;

			if (!await IsAllowedAsync(message))
			{
				// ignore admin commands from non-admins
				await API.SendMessage(Localization.Get($"annoy_{RNG.NextNoMemory(1, 10)}", senderChatId), senderChatId);
				return true;
			}

			ICommandAnswer result = null;

			// fire command's action
			if (ActionAsync != null)
				result = await ActionAsync(message);
			else if (Action != null)
				result = Action(message);

			// send result if we got any
			if (result != null)
			{
				var replyId = Reply ? message.MessageId : 0;
				Message sentMessage;
				switch (result)
				{
					case TextCommandAnswer answer:
						sentMessage = await API.SendMessage(answer.Text, senderChatId, ParseMode, replyId, answer.ReplyMarkup); break;
					case ImageCommandAnswer answer:
						sentMessage = await API.SendPhoto(answer.Image, senderChatId, answer.Caption, ParseMode, replyId, answer.ReplyMarkup, answer.SendTextIfFailed); break;
					case ImageAlbumCommandAnswer answer:
						sentMessage = (await API.SendMultiplePhotos(answer.Images, senderChatId))?.FirstOrDefault(); break;
					case StickerCommandAnswer answer:
						sentMessage = await API.SendSticker(answer.Sticker, senderChatId); break;
					default:
						throw new ArgumentException("Invalid command answer type");
				}

				if (sentMessage != null)
					ActionResult?.Invoke(sentMessage); // call action that receives sent message

				return true;
			}
			return false;
		}
	}
}
