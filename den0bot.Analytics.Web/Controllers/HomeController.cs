// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Analytics.Data;
using den0bot.Analytics.Web.Caches;
using den0bot.Analytics.Web.Models;
using Highsoft.Web.Mvc.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace den0bot.Analytics.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly TelegramBotClient telegramClient;

		private readonly DateTime minimal_date = new(2019, 10, 2);
		private const int last_message_days_ago = 60;

		public HomeController(TelegramBotClient _telegramClient)
		{
			telegramClient = _telegramClient;
		}

		public async Task<IActionResult> Index()
		{
			var model = new List<ShortChatModel>();
			await using (var db = new AnalyticsDatabase())
			{
				var chats = await db.Messages.AsNoTracking()
					.Where(x => x.Timestamp > DateTime.UtcNow.AddDays(-last_message_days_ago).Ticks)
					.GroupBy(x => x.ChatId)
					.Select(x => new{Id = x.Key, Msgs = x.Max(m => m.TelegramId)})
					.ToArrayAsync();

				foreach (var chat in chats)
				{
					var tgChat = await TelegramCache.GetChat(telegramClient, chat.Id);
					if (tgChat != null && tgChat.Type != ChatType.Private)
					{
						var lastMessageTimestamp = (await db.Messages.AsNoTracking()
							.Where(x => x.ChatId == chat.Id)
							.OrderByDescending(x => x.Timestamp)
							.FirstAsync()).Timestamp;

						model.Add(new ShortChatModel
						{
							Name = tgChat.Title,
							Avatar = await TelegramCache.GetChatImage(telegramClient, chat.Id, tgChat.Photo?.SmallFileId),
							Messages = chat.Msgs,
							Id = chat.Id,
							LastMessageTimestamp = new DateTime(lastMessageTimestamp)
						});
					}
				}
			}

			return View(model.OrderByDescending(x=>x.LastMessageTimestamp).ToArray());
		}

		public async Task<IActionResult> Chat(long id)
		{
			var model = new ChatModel { ChatId = id };
			await using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var chat = await TelegramCache.GetChat(telegramClient, id);
				if (chat == null && !await db.Messages.AnyAsync(x => x.ChatId == id))
					return NotFound();

				ViewData["Title"] = chat?.Title ?? "???";

				if (chat?.Photo != null)
				{
					ViewData["Image"] =
						await TelegramCache.GetChatImage(telegramClient, id, chat?.Photo?.SmallFileId);
				}

				var users = new List<ChatModel.UserTable.User>();

				var dbUsers = await db.UserStatsQuery.FromSqlInterpolated($@"SELECT UserId as Id, 
					COUNT(*) as Messages,
					SUM(CASE WHEN Command != '' THEN 1 ELSE 0 END) as Commands,
					SUM(CASE WHEN Command = '/devka' OR Command = '/seasonaldevka' THEN 1 ELSE 0 END) as GirlsRequested,
					SUM(CASE WHEN Type = 2 THEN 1 ELSE 0 END) as Stickers,
					SUM(CASE WHEN Type = 3 THEN 1 ELSE 0 END) as Voices,
					MAX(Timestamp) as LastMessageTimestamp
					FROM 'Messages' WHERE ChatId = {id} GROUP BY UserId
					ORDER BY Messages DESC").Where(x=> x.Messages > 1).ToArrayAsync();

				foreach (var user in dbUsers)
				{
					var name = "???";
					var username = string.Empty;
					var tgUser = await TelegramCache.GetUser(telegramClient, id, user.Id);
					if (tgUser != null)
					{
						name = $"{tgUser.FirstName} {tgUser.LastName}".Trim();
						if (!string.IsNullOrEmpty(tgUser.Username))
							username = tgUser.Username;
					}

					var girlsAdded = await db.Girls.CountAsync(x => x.UserId == user.Id && x.ChatId == id);

					var avgLength = await db.UserStatsAverageQuery.FromSqlInterpolated($@"SELECT AVG(Length) as AverageLength
					FROM (SELECT Length, NTILE(4) OVER (ORDER BY Length) n
						FROM (SELECT Length FROM Messages WHERE UserId = {user.Id} AND ChatId = {id} AND Command = '')
						)
					WHERE n IN (2,3)").SingleAsync();

					users.Add(new ChatModel.UserTable.User
					{
						Id = user.Id,
						Name = name,
						Username = username,
						Avatar = await TelegramCache.GetAvatar(telegramClient, user.Id),
						Messages = user.Messages - user.Commands,
						Commands = user.Commands,
						Stickers = user.Stickers,
						Voices = user.Voices,
						AverageLength = avgLength?.AverageLength ?? 0.0,
						LastMessageTime = TimeAgo(new DateTime(user.LastMessageTimestamp)),
						GirlsAdded = girlsAdded,
						GirlsRequested = user.GirlsRequested
					});
				}

				model.UsersTable.Users = users.ToArray();
			}

			return View(model);
		}

		[Route("user/{id:long}")]
		public async Task<IActionResult> GetUser(long id)
		{
			var model = new UserModel { UserId = id };
			await using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var user = await TelegramCache.GetUser(telegramClient, null, id);
				if (user == null)
					return NotFound();

				var title = $"{user.FirstName} {user.LastName}".Trim();
				if (!string.IsNullOrEmpty(user.Username)) 
					ViewData["Title"] = title + $" (@{user.Username})";
				else
					ViewData["Title"] = title;

				ViewData["Image"] = await TelegramCache.GetAvatar(telegramClient, id);

				var chats = new List<UserModel.ChatTable.Chat>();

				var dbChats = await db.UserStatsQuery.FromSqlInterpolated($@"SELECT ChatId as Id, 
					COUNT(*) as Messages,
					SUM(CASE WHEN Command != '' THEN 1 ELSE 0 END) as Commands,
					SUM(CASE WHEN Command = '/devka' OR Command = '/seasonaldevka' THEN 1 ELSE 0 END) as GirlsRequested,
					SUM(CASE WHEN Type = 2 THEN 1 ELSE 0 END) as Stickers,
					SUM(CASE WHEN Type = 3 THEN 1 ELSE 0 END) as Voices,
					MAX(Timestamp) as LastMessageTimestamp
					FROM 'Messages' WHERE UserId = {id} GROUP BY ChatId
					ORDER BY LastMessageTimestamp DESC;").ToArrayAsync();

				foreach (var chat in dbChats)
				{
					var tgChat = await TelegramCache.GetChat(telegramClient, chat.Id);
					var name = tgChat?.Title;
					if (string.IsNullOrEmpty(name))
						name = tgChat?.Id.ToString() ?? "???";

					chats.Add(new UserModel.ChatTable.Chat
					{
						Id = chat.Id,
						Name = name,
						Avatar = await TelegramCache.GetChatImage(telegramClient, chat.Id, tgChat?.Photo?.SmallFileId),
						Messages = chat.Messages - chat.Commands,
						Stickers = chat.Stickers,
						Voices = chat.Voices,
						LastMessageTime = TimeAgo(new DateTime(chat.LastMessageTimestamp))
					});
				}

				model.ChatsTable.Chats = chats.ToArray();
			}

			return View(model);
		}

		public async Task<IActionResult> TimesChartChat(long id, DateTime startTime, DateTime endTime)
		{
			var model = new ChartModel();

			if (endTime < minimal_date || endTime > DateTime.Today)
				endTime = DateTime.Today;

			if (startTime < minimal_date || startTime > DateTime.Today)
				startTime = endTime.AddDays(-6);

			await using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var days = (endTime - startTime).Days;
				var times = PrepareDates(days, endTime);

				List<string> timesStrings = new List<string>();
				times.ForEach(x => timesStrings.Add(x.ToShortDateString()));

				model.Times.XAxis = new XAxis { Categories = timesStrings };
				model.Times.YAxis = new YAxis { Title = new YAxisTitle {Text = "messages"} };
				model.Times.Series = new List<Series>();

				var userIds = await db.Messages.Where(x => x.ChatId == id).Select(x => x.UserId).Distinct().ToArrayAsync();
				var dayTuples = new List<Tuple<long, long>[]>(days);
				foreach (var time in times)
				{
					var dateTicks = time.Ticks;
					var endTicks = dateTicks + TimeSpan.TicksPerDay;
					if (days > 30)
						endTicks = dateTicks + TimeSpan.TicksPerDay * 7;

					var tuple = await db.Messages.Where(x => x.ChatId == id &&
															 x.Timestamp >= dateTicks &&
															 x.Timestamp < endTicks)
						.GroupBy(x => x.UserId)
						.Select(x => new Tuple<long, long>(x.Key, x.LongCount()))
						.ToArrayAsync();
					dayTuples.Add(tuple);
				}

				foreach (var userId in userIds)
				{
					var name = "???";

					var tgUser = await TelegramCache.GetUser(telegramClient, id, userId);
					if (tgUser != null)
					{
						if (string.IsNullOrEmpty(tgUser.Username))
							name = $"{tgUser.FirstName} {tgUser.LastName}".Trim();
						else
							name = tgUser.Username;
					}

					var messageData = new List<LineSeriesData>();

					foreach (var dayTuple in dayTuples)
					{
						var dayMessages = dayTuple.Where(x => x.Item1 == userId).Select(x=> x.Item2).SingleOrDefault();
						messageData.Add(new LineSeriesData { Y = dayMessages });
					}

					model.Times.Series.Add(new LineSeries
					{
						Name = name,
						Data = messageData
					});
				}
			}

			return PartialView("TimesChart", model);
		}

		public async Task<IActionResult> TimesChartUser(long id, DateTime startTime, DateTime endTime)
		{
			var model = new ChartModel();

			if (endTime < minimal_date || endTime > DateTime.Today)
				endTime = DateTime.Today;

			if (startTime < minimal_date || startTime > DateTime.Today)
				startTime = endTime.AddDays(-6);

			await using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var days = (endTime - startTime).Days;
				var times = PrepareDates(days, endTime);

				List<string> timesStrings = new List<string>();
				times.ForEach(x => timesStrings.Add(x.ToShortDateString()));

				model.Times.XAxis = new XAxis { Categories = timesStrings };
				model.Times.YAxis = new YAxis { Title = new YAxisTitle { Text = "messages" } };
				model.Times.Series = new List<Series>();

				var chatIds = await db.Messages.Where(x => x.UserId == id).Select(x => x.ChatId).Distinct().ToArrayAsync();
				var dayTuples = new List<Tuple<long, long>[]>(days);
				foreach (var time in times)
				{
					var dateTicks = time.Ticks;
					var endTicks = dateTicks + TimeSpan.TicksPerDay;
					if (days > 30)
						endTicks = dateTicks + TimeSpan.TicksPerDay * 7;

					var tuple = await db.Messages.Where(x => x.UserId == id &&
															 x.Timestamp >= dateTicks &&
															 x.Timestamp < endTicks)
						.GroupBy(x => x.ChatId)
						.Select(x => new Tuple<long, long>(x.Key, x.LongCount()))
						.ToArrayAsync();

					dayTuples.Add(tuple);
				}

				foreach (var chatId in chatIds)
				{
					var name = "???";

					var tgChat = await TelegramCache.GetChat(telegramClient, chatId);
					if (tgChat != null)
					{
						if (string.IsNullOrEmpty(tgChat.Title))
							name = tgChat.Id.ToString();
						else
							name = tgChat.Title;
					}

					var messageData = new List<LineSeriesData>();

					foreach (var dayTuple in dayTuples)
					{
						var dayMessages = dayTuple.Where(x => x.Item1 == chatId).Select(x => x.Item2).SingleOrDefault();
						messageData.Add(new LineSeriesData { Y = dayMessages });
					}

					model.Times.Series.Add(new LineSeries
					{
						Name = name,
						Data = messageData
					});
				}
			}

			return PartialView("TimesChart", model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
		}

		private static List<DateTime> PrepareDates(int days, DateTime endTime)
		{
			var times = new List<DateTime>();
			for (var i = 0; i <= days; i++)
			{
				// count weeks if we have more 30 days
				if (days > 30 && i % 7 != 0)
					continue;

				times.Add(endTime.AddDays(-i));
			}

			times.Reverse();
			return times;
		}

		private static string TimeAgo(DateTime dt)
		{
			if (dt > DateTime.Now.ToUniversalTime())
				return "soon";
			TimeSpan span = DateTime.Now.ToUniversalTime() - dt;

			switch (span)
			{
				case var _ when span.Days > 365:
				{
					int years = (span.Days / 365);
					if (span.Days % 365 != 0)
						years += 1;
					return $"about {years} {(years == 1 ? "year" : "years")} ago";
				}
				case var _ when span.Days > 30:
				{
					int months = (span.Days / 30);
					if (span.Days % 31 != 0)
						months += 1;
					return $"about {months} {(months == 1 ? "month" : "months")} ago";
				}

				case var _ when span.Days > 0:
					return $"about {span.Days} {(span.Days == 1 ? "day" : "days")} ago";

				case var _ when span.Hours > 0:
					return $"about {span.Hours} {(span.Hours == 1 ? "hour" : "hours")} ago";

				case var _ when span.Minutes > 0:
					return $"about {span.Minutes} {(span.Minutes == 1 ? "minute" : "minutes")} ago";

				case var _ when span.Seconds > 5:
					return $"about {span.Seconds} seconds ago";

				case var _ when span.Seconds <= 5:
					return "just now";

				default: return string.Empty;
			}
		}
	}
}