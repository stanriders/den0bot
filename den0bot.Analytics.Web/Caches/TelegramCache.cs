// den0bot (c) StanR 2021 - MIT License
using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace den0bot.Analytics.Web.Caches
{
	public static class TelegramCache
	{
		private static readonly MemoryCache cache = new("tgCache");
		private static readonly MemoryCache nullCache = new("nullCache"); // kinda stupid but re-querying api for nonexisting users/chats is taking too much time

		public static async Task<User> GetUser(TelegramBotClient client, long? chatId, long userId)
		{
			var cacheKey = $"user_{userId.ToString()}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as User;

			if (chatId == null || nullCache.Contains(cacheKey))
				return null;

			try
			{
				var user = (await client.GetChatMemberAsync(chatId, (int)userId)).User;
				cache.Add(cacheKey, user, DateTimeOffset.Now.AddDays(1));
				return user;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		public static async Task<Chat> GetChat(TelegramBotClient client, long chatId)
		{
			var cacheKey = $"chat_{chatId.ToString()}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as Chat;

			if (nullCache.Contains(cacheKey))
				return null;

			try
			{
				var chat = await client.GetChatAsync(chatId);
				cache.Add(cacheKey, chat, DateTimeOffset.Now.AddDays(1));
				return chat;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		public static async Task<string> GetChatImage(TelegramBotClient client, long chatId, string fileId)
		{
			var cacheKey = $"chatImg_{chatId.ToString()}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as string;

			if (nullCache.Contains(cacheKey))
				return null;

			try
			{
				var imageLocalPath = $"./wwwroot/img/{chatId}.png";
				await DownloadTelegramFile(client, fileId, imageLocalPath);

				var image = string.Empty;
				if (System.IO.File.Exists(imageLocalPath))
					image = $"/img/{chatId}.png";

				cache.Add(cacheKey, image, DateTimeOffset.Now.AddDays(1));
				return image;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		public static async Task<string> GetAvatar(TelegramBotClient client, long userId)
		{
			var cacheKey = $"avatar_{userId.ToString()}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as string;

			if (nullCache.Contains(cacheKey))
				return null;

			var avatarLocalPath = $"./wwwroot/img/{userId}.png";

			try
			{
				var avatarId = await client.GetUserProfilePhotosAsync((int)userId, 0, 1);
				if (avatarId?.Photos != null && avatarId?.Photos.Length > 0)
					await DownloadTelegramFile(client, avatarId.Photos[0][0].FileId, avatarLocalPath);

				var avatar = string.Empty;
				if (System.IO.File.Exists(avatarLocalPath))
					avatar = $"/img/{userId}.png";

				cache.Add(cacheKey, avatar, DateTimeOffset.Now.AddDays(1));
				return avatar;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		private static async Task DownloadTelegramFile(TelegramBotClient client, string fileId, string path)
		{
			if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(path))
				return;

			using (var stream = new MemoryStream())
			{
				var file = await client.GetFileAsync(fileId);
				await client.DownloadFileAsync(file.FilePath, stream);
				stream.Position = 0;

				byte[] buf = new byte[stream.Length];
				await stream.ReadAsync(buf, 0, (int)stream.Length);

				await System.IO.File.WriteAllBytesAsync(path, buf);
			}
		}
	}
}
