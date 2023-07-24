// den0bot (c) StanR 2023 - MIT License
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using den0bot.Analytics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = den0bot.Analytics.Data.Types.User;

namespace den0bot.Analytics.Web.Caches
{
	public class TelegramCache : ITelegramCache
	{
		private readonly MemoryCache cache = new("tgCache");
		private readonly MemoryCache nullCache = new("nullCache"); // kinda stupid but re-querying api for nonexisting users/chats is taking too much time

		private readonly ITelegramBotClient client;
		private readonly ILogger<ITelegramCache> logger;

		public TelegramCache(ILogger<ITelegramCache> _logger, ITelegramBotClient _client)
		{
			logger = _logger;
			client = _client;
			PopulateCache();
		}

		private void PopulateCache()
		{
			logger.LogInformation("Populating telegram cache...");
			using var db = new AnalyticsDatabase();

			var users = db.Users.ToArray();
			foreach (var user in users)
			{
				// keep db data cached for a short time
				cache.Add($"user_{user.Id}", user, DateTimeOffset.Now.AddHours(1));
			}
		}

		public async Task<User> GetUser(long? chatId, long userId)
		{
			var cacheKey = $"user_{userId}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as User;

			if (chatId == null || nullCache.Contains(cacheKey))
				return null;

			try
			{
				await using var db = new AnalyticsDatabase();

				var tgUser = (await client.GetChatMemberAsync(chatId, (int)userId)).User;
				var dbUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
				if (dbUser != null)
				{
					dbUser.FirstName = tgUser.FirstName;
					dbUser.LastName = tgUser.LastName;
					dbUser.Username = tgUser.Username;
					db.Users.Update(dbUser);
				}
				else
				{
					dbUser = new User
					{
						Id = tgUser.Id,
						Username = tgUser.Username,
						FirstName = tgUser.FirstName,
						LastName = tgUser.LastName
					};
					await db.Users.AddAsync(dbUser);
				}

				await db.SaveChangesAsync();

				cache.Add(cacheKey, dbUser, DateTimeOffset.Now.AddDays(1));
				return dbUser;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to GetUser");
				await using var db = new AnalyticsDatabase();

				var dbUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
				if (dbUser != null)
				{
					cache.Add(cacheKey, dbUser, DateTimeOffset.Now.AddHours(1));
				}
				else
				{
					nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				}

				return dbUser;
			}
		}

		public async Task<Chat> GetChat(long chatId)
		{
			var cacheKey = $"chat_{chatId}";

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
				logger.LogError(e, "Failed to GetChat");
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		public async Task<string> GetChatImage(long chatId, string fileId)
		{
			var cacheKey = $"chatImg_{chatId}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as string;

			if (nullCache.Contains(cacheKey))
				return null;

			try
			{
				var imageLocalPath = $"./wwwroot/img/{chatId}.png";
				await DownloadTelegramFile(fileId, imageLocalPath);

				var image = string.Empty;
				if (System.IO.File.Exists(imageLocalPath))
					image = $"/img/{chatId}.png";

				cache.Add(cacheKey, image, DateTimeOffset.Now.AddDays(1));
				return image;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to GetChatImage");
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		public async Task<string> GetAvatar(long userId)
		{
			var cacheKey = $"avatar_{userId}";

			if (cache.Contains(cacheKey))
				return cache[cacheKey] as string;

			if (nullCache.Contains(cacheKey))
				return null;

			var avatarLocalPath = $"./wwwroot/img/{userId}.png";

			try
			{
				var avatarId = await client.GetUserProfilePhotosAsync((int)userId, 0, 1);
				if (avatarId.Photos.Length > 0)
					await DownloadTelegramFile(avatarId.Photos[0][0].FileId, avatarLocalPath);

				var avatar = string.Empty;
				if (System.IO.File.Exists(avatarLocalPath))
					avatar = $"/img/{userId}.png";

				cache.Add(cacheKey, avatar, DateTimeOffset.Now.AddDays(1));
				return avatar;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to GetAvatar");
				nullCache.Add(cacheKey, cacheKey, DateTimeOffset.Now.AddHours(1));
				return null;
			}
		}

		private async Task DownloadTelegramFile(string fileId, string path)
		{
			if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(path))
				return;

			await using var stream = new MemoryStream();

			var file = await client.GetFileAsync(fileId);
			await client.DownloadFileAsync(file.FilePath, stream);

			await System.IO.File.WriteAllBytesAsync(path, stream.ToArray());
		}
	}
}