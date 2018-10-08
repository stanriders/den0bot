// den0bot (c) StanR 2018 - MIT License
using den0bot.Util;

namespace den0bot
{
    public static class Events
    {
        public static string Event(long chatID)
        {
            switch (RNG.NextNoMemory(0, 1000))
            {
                case 9:		return Localization.Get("event_1", chatID);
                case 99:	return Localization.Get("event_2", chatID);
                case 999:	return Localization.Get("event_3", chatID);
                case 8:		return Localization.Get("event_4", chatID);
                case 88:	return Localization.Get("event_5", chatID);
                case 888:	return Localization.Get("event_6", chatID);
                case 7:		return Localization.Get("event_7", chatID);
                case 77:	return Localization.Get("event_8", chatID);
                case 777:	return Localization.Get("event_9", chatID);
                default:	return string.Empty;
            }
        }
        public static string Annoy(long chatID)
        {
            switch (RNG.NextNoMemory(0, 9))
            {
                case 0: return Localization.Get("annoy_1", chatID);
                case 1: return Localization.Get("annoy_2", chatID);
                case 2: return Localization.Get("annoy_3", chatID);
                case 3: return Localization.Get("annoy_4", chatID);
                case 4: return Localization.Get("annoy_5", chatID);
                case 5: return Localization.Get("annoy_6", chatID);
                case 6: return Localization.Get("annoy_7", chatID);
                case 7: return Localization.Get("annoy_8", chatID);
                case 8: return Localization.Get("annoy_9", chatID);
                default: return string.Empty;
            }
        }
        public static string RatingRepeat(long chatID)
        {
            switch (RNG.NextNoMemory(0, 9))
            {
                case 0: return Localization.Get("rating_repeat_1", chatID);
                case 1: return Localization.Get("rating_repeat_2", chatID);
                case 2: return Localization.Get("rating_repeat_3", chatID);
                case 3: return Localization.Get("rating_repeat_4", chatID);
                case 4: return Localization.Get("rating_repeat_5", chatID);
                case 5: return Localization.Get("rating_repeat_6", chatID);
                case 6: return Localization.Get("rating_repeat_7", chatID);
                case 7: return Localization.Get("rating_repeat_8", chatID);
                case 8: return Localization.Get("rating_repeat_9", chatID);
                default: return string.Empty;
            }
        }
    }
}
