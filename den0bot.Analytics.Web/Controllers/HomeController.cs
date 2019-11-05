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
using Telegram.Bot.Types;

namespace den0bot.Analytics.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly TelegramBotClient telegramClient;

		private readonly DateTime minimal_date = new DateTime(2019, 10, 2);
		public HomeController(TelegramBotClient _telegramClient)
		{
			telegramClient = _telegramClient;
		}

		public async Task<IActionResult> Index()
		{
			var model = new List<ShortChatModel>();
			using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var chats = await db.Messages.GroupBy(x => x.ChatId)
					.Select(x => new{Id = x.Key, Msgs = x.Max(m => m.TelegramId)})
					.ToArrayAsync();

				foreach (var chat in chats)
				{
					var tgChat = await TelegramCache.GetChat(telegramClient, chat.Id);
					if (tgChat != null)
					{
						model.Add(new ShortChatModel
						{
							Name = tgChat.Title,
							Avatar =
								await TelegramCache.GetChatImage(telegramClient, chat.Id, tgChat.Photo?.SmallFileId),
							Messages = chat.Msgs,
							Id = chat.Id
						});
					}
				}
			}

			return View(model.OrderByDescending(x=>x.Messages).ToArray());
		}

		public async Task<IActionResult> Chat(long chatId)
		{
			var model = new ChatModel { ChatId = chatId };
			using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				Chat chat = await TelegramCache.GetChat(telegramClient, chatId);
				if (chat == null)
					return NotFound();

				ViewData["Title"] = chat.Title;

				if (chat.Photo != null)
				{
					ViewData["Image"] =
						await TelegramCache.GetChatImage(telegramClient, chatId, chat.Photo.SmallFileId);
				}

				var users = new List<ChatModel.UserTable.User>();

				var dbUsers = await db.UserStatsQuery.FromSqlInterpolated($@"SELECT UserId as Id, 
					COUNT(*) as Messages,
					SUM(CASE WHEN Command = '/devka' OR Command = '/seasonaldevka' THEN 1 ELSE 0 END) as GirlsRequested,
					SUM(CASE WHEN Type = 2 THEN 1 ELSE 0 END) as Stickers,
					MAX(Timestamp) as LastMessageTimestamp
					FROM 'Messages' WHERE ChatId = {chatId} GROUP BY UserId
					ORDER BY Messages DESC;").ToArrayAsync();

				foreach (var user in dbUsers)
				{
					var tgUser = await TelegramCache.GetUser(telegramClient, chatId, user.Id);
					var name = tgUser?.Username;
					if (string.IsNullOrEmpty(name))
						name = tgUser?.FirstName ?? "???";

					var girlsAdded = await db.Girls.CountAsync(x => x.UserId == user.Id && x.ChatId == chatId);

					var avgLength = await db.UserStatsAverageQuery.FromSqlInterpolated($@"SELECT AVG(Length) as AverageLength
					FROM (SELECT Length, NTILE(4) OVER (ORDER BY Length) n
						FROM (SELECT Length FROM Messages WHERE UserId = {user.Id} AND ChatId = {chatId})
						)
					WHERE n IN (2,3)").SingleAsync();

					users.Add(new ChatModel.UserTable.User
					{
						Name = name,
						Avatar = await TelegramCache.GetAvatar(telegramClient, user.Id),
						Messages = user.Messages,
						Stickers = user.Stickers,
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

		public async Task<IActionResult> TimesChart(long chatId, DateTime startTime, DateTime endTime)
		{
			var model = new ChartModel();

			if (endTime < minimal_date || endTime > DateTime.Today)
				endTime = DateTime.Today;

			if (startTime < minimal_date || startTime > DateTime.Today)
				startTime = endTime.AddDays(-6);

			using (var db = new AnalyticsDatabase())
			{
				db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

				var times = new List<DateTime>();
				var days = (endTime - startTime).Days;
				for (var i = 0; i <= days; i++)
				{
					times.Add(endTime.AddDays(-i));
				}

				times.Reverse();

				List<string> timesStrings = new List<string>();
				times.ForEach(x => timesStrings.Add(x.ToShortDateString()));

				model.Times.XAxis = new XAxis { Categories = timesStrings };
				model.Times.YAxis = new YAxis { Title = new YAxisTitle {Text = "messages"} };
				model.Times.Series = new List<Series>();

				var userIds = await db.Messages.Where(x => x.ChatId == chatId).Select(x => x.UserId).Distinct().ToArrayAsync();
				var dayTuples = new List<Tuple<long, long>[]>(days);
				foreach (var time in times)
				{
					var dateTicks = time.Ticks;
					var tuple = await db.Messages.Where(x => x.ChatId == chatId &&
															 x.Timestamp >= dateTicks &&
															 x.Timestamp <
															 dateTicks + TimeSpan.TicksPerDay)
						.GroupBy(x => x.UserId)
						.Select(x => new Tuple<long, long>(x.Key, x.LongCount()))
						.ToArrayAsync();
					dayTuples.Add(tuple);
				}

				foreach (var userId in userIds)
				{
					var tgUser = await TelegramCache.GetUser(telegramClient, chatId, userId);
					if (tgUser == null)
						continue;

					var name = tgUser.Username;
					if (string.IsNullOrEmpty(name))
						name = tgUser.FirstName;

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

			return PartialView(model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
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