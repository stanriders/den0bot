// den0bot (c) StanR 2021 - MIT License
using System;
using Telegram.Bot.Types;

namespace den0bot.Events
{
	public class MessageEventArgs : EventArgs
	{
		public Message Message { get; }

		public MessageEventArgs(Message message) => Message = message;
	}
}
