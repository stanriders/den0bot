// den0bot (c) StanR 2019 - MIT License
using den0bot.Util;

namespace den0bot
{
	public static class Events
	{
		public static string Event(long chatID)
		{
			switch (RNG.NextNoMemory(0, 1000))
			{
				case 9: return Localization.Get("event_1", chatID);
				case 99: return Localization.Get("event_2", chatID);
				case 999: return Localization.Get("event_3", chatID);
				case 8: return Localization.Get("event_4", chatID);
				case 88: return Localization.Get("event_5", chatID);
				case 888: return Localization.Get("event_6", chatID);
				case 7: return Localization.Get("event_7", chatID);
				case 77: return Localization.Get("event_8", chatID);
				case 777: return Localization.Get("event_9", chatID);
				default: return string.Empty;
			}
		}
	}
}
