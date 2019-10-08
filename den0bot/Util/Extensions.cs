﻿// den0bot (c) StanR 2019 - MIT License
using System;
using System.Text.RegularExpressions;
using den0bot.Analytics.Data.Types;

namespace den0bot.Util
{
	public static class Extensions
	{
		public static string FilterHTML(this string value)
		{
			var step1 = value.Replace("<br>", Environment.NewLine);

			var step2 = Regex.Replace(step1, @"<[^>]+>", "").Trim();

			return Regex.Replace(step2, @"\s{2,}", " ")
						.Replace("&gt;", ">")
						.Replace("&nbsp;", " ")
						.Replace("&quot;", "\"")
						.Replace("&#47;", "/");
		}

		public static string FilterToHTML(this string value)
		{
			return value.Replace("&", "&amp;")
						.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("\"", "&quot;");
		}

		public static string InnerMessageIfAny(this Exception value)
		{
			return value.InnerException?.Message ?? value.Message;
		}

		public static MessageType ToDbType(this Telegram.Bot.Types.Enums.MessageType msg)
		{
			return msg switch
			{
				Telegram.Bot.Types.Enums.MessageType.Text => MessageType.Text,
				Telegram.Bot.Types.Enums.MessageType.Photo => MessageType.Photo,
				Telegram.Bot.Types.Enums.MessageType.Sticker => MessageType.Sticker,
				_ => MessageType.Other
			};
		}
	}
}
