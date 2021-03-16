// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using System.IO;
using System.Linq;
using den0bot.DB;
using den0bot.Types;
using den0bot.Types.Answers;
using Newtonsoft.Json;

namespace den0bot.Util
{
	public static class Localization
	{
		public static List<string> GetAvailableLocales() => locales.Keys.ToList();
		private static readonly Dictionary<string, Dictionary<string, string>> locales = new();

		static Localization()
		{
			var localeFiles = Directory.GetFiles("./Locales");
			foreach (var localeFile in localeFiles)
			{
				if (localeFile.EndsWith(".json"))
				{
					var locale =
						JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(localeFile));
					var localeName = Path.GetFileName(localeFile).Replace(".json", "");

					locales.Add(localeName, locale);
				}
			}
		}

		public static string FormatGet(string key, long chatID, params object[] arg)
		{
			return string.Format(Get(key, chatID), arg);
		}

		public static string Get(string key, long chatID)
		{
			string locale = DatabaseCache.Chats.FirstOrDefault(x => x.Id == chatID)?.Locale;
			if (locale != null && locales.ContainsKey(locale))
			{
				if (locales[locale].ContainsKey(key))
					return locales[locale][key];
			}
			// defaults to russian (assume its complete)
			return locales["ru"][key];
		}

		public static TextCommandAnswer GetAnswer(string key, long chatID)
		{
			return new TextCommandAnswer(Get(key,chatID));
		}

		public static TextCommandAnswer GetAnswerFormat(string key, long chatID, params object[] arg)
		{
			return new TextCommandAnswer(FormatGet(key, chatID, arg));
		}

		public static string NewMemberGreeting(long chatID, string name, long userID)
		{
			var chat = DatabaseCache.Chats.FirstOrDefault(x => x.Id == chatID);

			var introduction = chat?.Introduction;
			if (!string.IsNullOrEmpty(introduction))
				return string.Format(introduction, name, userID);

			string locale = chat?.Locale;
			if (locale != null && locales.ContainsKey(locale))
				return string.Format(locales[locale]["generic_greeting"], name, userID);
			else
				return string.Format(locales["ru"]["generic_greeting"], name, userID);
		}
	}
}
