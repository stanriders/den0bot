// den0bot (c) StanR 2021 - MIT License
using System;
using Telegram.Bot.Types;

namespace den0bot.Events
{
	public class MessageEditEventArgs : EventArgs
	{
		public Message EditedMessage { get; }

		public MessageEditEventArgs(Message message) => EditedMessage = message;
	}
}
