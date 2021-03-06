﻿// den0bot (c) StanR 2020 - MIT License
using System;
using System.ComponentModel;
using den0bot.Analytics.Data.Types;

namespace den0bot.Util
{
	public static class Extensions
	{
		public static string FilterToHTML(this string value)
		{
			return value.Replace("&", "&amp;")
						.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("\"", "&quot;");
		}

		public static string InnerMessageIfAny(this Exception value)
		{
			return value.InnerException?.ToString() ?? value.ToString();
		}

		public static MessageType ToDbType(this Telegram.Bot.Types.Enums.MessageType msg)
		{
			return msg switch
			{
				Telegram.Bot.Types.Enums.MessageType.Text => MessageType.Text,
				Telegram.Bot.Types.Enums.MessageType.Photo => MessageType.Photo,
				Telegram.Bot.Types.Enums.MessageType.Sticker => MessageType.Sticker,
				Telegram.Bot.Types.Enums.MessageType.Voice => MessageType.Voice,
				Telegram.Bot.Types.Enums.MessageType.VideoNote => MessageType.Voice,
				Telegram.Bot.Types.Enums.MessageType.Video => MessageType.Video,
				_ => MessageType.Other
			};
		}

		public static string Capitalize(this string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
		}

		public static string GetDescription<T>(this T source)
		{
			var fi = source.GetType().GetField(source.ToString());

			var attributes = (DescriptionAttribute[])fi?.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes != null && attributes.Length > 0) 
				return attributes[0].Description;
			else 
				return source.ToString();
		}
	}
}
