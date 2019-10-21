// den0bot (c) StanR 2019 - MIT License
using System.Collections.Generic;
using System.IO;
using System.Linq;
using den0bot.DB;
using Newtonsoft.Json;

namespace den0bot.Util
{
	public static class Localization
	{
		public static List<string> GetAvailableLocales() => locales.Keys.ToList();
		private static readonly Dictionary<string, Dictionary<string, string>> locales = new Dictionary<string, Dictionary<string, string>>();

		private static bool isReady;
		private static void Init()
		{
			if (!isReady)
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

				isReady = true;
			}
		}

		public static string FormatGet(string key, long chatID, params object[] arg)
		{
			Init();
			return string.Format(Get(key, chatID), arg);
		}

		public static string Get(string key, long chatID)
		{
			Init();
			string locale = Database.GetChatLocale(chatID);
			if (locales.ContainsKey(locale))
			{
				if (locales[locale].ContainsKey(key))
					return locales[locale][key];
			}
			// defaults to russian (assume its complete)
			return locales["ru"][key];
		}

		public static string NewMemberGreeting(long chatID, string name, long userID)
		{
			Init();
			string locale = Database.GetChatLocale(chatID);
			if (locales.ContainsKey(locale))
				return string.Format(locales[locale]["generic_greeting"], name, userID);
			else
				return string.Format(locales["ru"]["generic_greeting"], name, userID);
		}
	}
}
