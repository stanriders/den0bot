// den0bot (c) StanR 2021 - MIT License
using System;
using Telegram.Bot.Types;

namespace den0bot.Events
{
	public class CallbackQueryEventArgs : EventArgs
	{
		public CallbackQuery CallbackQuery { get; }

		public CallbackQueryEventArgs(CallbackQuery callbackQuery) => CallbackQuery = callbackQuery;
	}
}
