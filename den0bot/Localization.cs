// den0bot (c) StanR 2018 - MIT License
using System.Collections.Generic;
using System.Linq;
using den0bot.DB;
using den0bot.Locales;

namespace den0bot
{
	public static class Localization
	{
		public static List<string> GetAvailableLocales() => locales.Keys.ToList();
		private static readonly Dictionary<string, ILocale> locales = new Dictionary<string, ILocale>
		{
			["ru"] = new RU(),
			["en"] = new EN(),
		};

		public static string FormatGet(string key, object arg, long chatID)
		{
			return string.Format(Get(key, chatID), arg);
		}

		public static string Get(string key, long chatID)
		{
			string locale = Database.GetChatLocale(chatID);
			if (locales.ContainsKey(locale))
			{
				if (locales[locale].Contains(key))
					return locales[locale].GetLocalizedString(key);
			}
			// defaults to russian (assume its complete)
			return locales["ru"].GetLocalizedString(key);
		}

		public static string NewMemberGreeting(long chatID, string name, long userID)
		{
			string locale = Database.GetChatLocale(chatID);
			if (locales.ContainsKey(locale))
				return locales[locale].NewMemberGreeting(name, userID);
			else
				return locales["ru"].NewMemberGreeting(name, userID);
		}
	}
}
