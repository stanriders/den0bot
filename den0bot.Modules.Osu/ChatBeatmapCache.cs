// den0bot (c) StanR 2021 - MIT License

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace den0bot.Modules.Osu
{
	public static class ChatBeatmapCache
	{
		public class CachedBeatmap
		{
			public uint BeatmapId { get; set; }
			public uint BeatmapSetId { get; set; }
		}

		private static readonly Dictionary<long, CachedBeatmap> lastMapCache = new();

		private static readonly MemoryCache sentMapsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep maps in cache

		public static CachedBeatmap GetLastMap(long chatId)
		{
			if (lastMapCache.ContainsKey(chatId))
				return lastMapCache[chatId];

			return null;
		}

		public static void StoreLastMap(long chatId, CachedBeatmap map)
		{
			if (lastMapCache.ContainsKey(chatId))
				lastMapCache[chatId] = map;
			else
				lastMapCache.Add(chatId, map);
		}

		public static void StoreSentMap(int messageId, CachedBeatmap map)
		{
			sentMapsCache.Add(messageId.ToString(), map, DateTimeOffset.Now.AddDays(days_to_keep_messages));
		}

		public static CachedBeatmap GetSentMap(int messageId)
		{
			return sentMapsCache.Get(messageId.ToString()) as CachedBeatmap;
		}
	}
}
