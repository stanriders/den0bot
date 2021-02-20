// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;

namespace den0bot.Modules.Osu
{
	public static class ChatBeatmapCache
	{
		private static readonly Dictionary<long, uint> lastMapCache = new();

		public static uint GetMap(long chatId)
		{
			if (lastMapCache.ContainsKey(chatId))
				return lastMapCache[chatId];

			return 0;
		}

		public static void StoreMap(long chatId, uint mapId)
		{
			if (lastMapCache.ContainsKey(chatId))
				lastMapCache[chatId] = mapId;
			else
				lastMapCache.Add(chatId, mapId);
		}
	}
}
