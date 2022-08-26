// den0bot (c) StanR 2022 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using den0bot.DB;
using den0bot.DB.Types;
using den0bot.Util;
using Microsoft.EntityFrameworkCore;
using den0bot.Types;
using den0bot.Types.Answers;

namespace den0bot.Modules
{
	internal class ModGirls : IModule, IReceiveAllMessages, IReceiveCallbacks
	{
		private class SentGirl
		{
			public int ID { get; init; }
			public int Rating { get; set; }
			public List<long> Voters { get; init; }
			public DateTime PostTime { get; init; }
			public int MessageId { get; init; }
			public int CommandMessageId { get; init; }
			public bool Seasonal { get; init; }
			public int Season { get; init; }
			public int SeasonalRating { get; set; }
		}

		private readonly MemoryCache sentGirlsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep girls in cache

		private readonly InlineKeyboardMarkup buttons = new(
			new [] { new InlineKeyboardButton("+") { CallbackData = "+" },
					 new InlineKeyboardButton("-") { CallbackData = "-" },
			}
		);

		private readonly Dictionary<long, Queue<SentGirl>> antispamBuffer = new(); // chatID, queue
		private const int antispam_cooldown = 15; // seconds
		private const int antispam_buffer_capacity = 4;

		private const int top_girls_amount = 9;
		private const int delete_rating_threshold = -10; // lowest rating a girl can have before completely removing her from db
		private const int max_reroll_iterations = 10;

		public ModGirls()
		{
			AddCommands(new[]
			{
				new Command
				{
					Names = { "devka", "tyanochku", "girl" },
					ActionAsync = msg => GetRandomGirl(msg),
					//ActionResult = msg => CacheGirl(msg)
				},
				new Command
				{
					Names = {"bestdevka", "bestgirl"},
					ActionAsync = (msg) => GetRandomPlatinumGirl(msg.Chat),
				},
				new Command
				{
					Names = {"topdevok", "topgirls" },
					ActionAsync = (msg) => TopGirls(msg.Chat.Id)
				},
				new Command
				{
					Names = {"antitopdevok", "antitopgirls" },
					ActionAsync = (msg) => TopGirls(msg.Chat.Id, true)
				},
				new Command
				{
					Name = "delet",
					IsAdminOnly = true,
					Reply = true,
					ActionAsync = DeleteGirl
				},
				new Command
				{
					Names = { "seasonaldevka", "seasonaltyanochku", "seasonalgirl" },
					ActionAsync = msg => GetRandomGirl(msg, true),
				},
				new Command
				{
					Names = { "seasonaltopdevok", "seasonaltopgirls" },
					ActionAsync = TopGirlsSeasonal
				},
				new Command
				{
					Name = "migrategirls",
					IsOwnerOnly = true,
					ActionAsync = Migrate
				}
			});
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Caption))
			{
				if (message.Type == MessageType.Photo &&
				    message.Caption == Localization.Get("girls_tag", message.Chat.Id))
				{
					await using (var db = new Database())
					{
						var link = message.Photo.Last().FileId;
						if (!db.Girls.Any(x => x.Link == link))
						{
							var season = DatabaseCache.GetGirlSeason();

							await db.Girls.AddAsync(new Girl
							{
								Link = link,
								ChatID = message.Chat.Id,
								Rating = 0,
								Season = season + 1, // next season
								SeasonRating = 0
							});

							await db.SaveChangesAsync();
						}
					}

					await ModAnalytics.AddGirl(message.Chat.Id, message.From.Id);
				}
			}
		}

		private async Task<ICommandAnswer> GetRandomGirl(Message msg, bool seasonal = false)
		{
			var chatId = msg.Chat.Id;
			await using var db = new Database();

			if (await db.Girls.CountAsync(x=> x.ChatID == chatId) <= 0)
				return Localization.GetAnswer("girls_not_found", chatId);

			if (!antispamBuffer.ContainsKey(chatId))
				antispamBuffer.Add(chatId, new Queue<SentGirl>(antispam_buffer_capacity));

			var picture = seasonal ? await GetGirlSeasonal(chatId) : await GetGirl(chatId);
			if (picture != null && picture.Link != string.Empty)
			{
				var sentMessage = await API.SendPhoto(picture.Link, 
					chatId, 
					seasonal ? $"{picture.SeasonRating} (s{picture.Season})" : picture.Rating.ToString(), 
					null, 
					0, 
					buttons,
					false);

				if (sentMessage != null)
				{
					var girl = new SentGirl
					{
						ID = picture.Id,
						Rating = picture.Rating == int.MinValue ? 0 : picture.Rating,
						Voters = new List<long>(),
						PostTime = DateTime.Now,
						MessageId = sentMessage.MessageId,
						CommandMessageId = msg.MessageId,
						Seasonal = seasonal,
						Season = DatabaseCache.GetGirlSeason(),
						SeasonalRating = picture.SeasonRating == int.MinValue ? 0 : picture.SeasonRating
					};
					sentGirlsCache.Add(sentMessage.MessageId.ToString(), girl, DateTimeOffset.Now.AddDays(days_to_keep_messages));
					antispamBuffer[chatId].Enqueue(girl);

					if (antispamBuffer[chatId].Count == antispam_buffer_capacity)
					{
						// check if third girl in a queue was posted less than antispam_cooldown seconds ago and remove it
						var oldestGirl = antispamBuffer[chatId].Dequeue();
						var cd = oldestGirl.PostTime.AddSeconds(antispam_cooldown);
						if (cd > DateTime.Now)
						{
							sentGirlsCache.Remove(oldestGirl.MessageId.ToString());
							await API.RemoveMessage(chatId, oldestGirl.MessageId);
							await API.RemoveMessage(chatId, oldestGirl.CommandMessageId);
						}
					}
				}

				return new EmptyCommandAnswer();
			}

			return Localization.GetAnswer("generic_fail", chatId);
		}

		private async Task<ICommandAnswer> GetRandomPlatinumGirl(Telegram.Bot.Types.Chat sender)
		{
			await using var db = new Database();

			var girl = await db.Girls
				.Where(x => x.ChatID == sender.Id && x.Rating >= 10)
				.OrderBy(r => EF.Functions.Random())
				.FirstOrDefaultAsync();

			if (girl is null)
				return Localization.GetAnswer("girls_not_found", sender.Id);

			return new ImageCommandAnswer
			{
				Image = girl.Link,
				SendTextIfFailed = false
			};
		}

		private async Task<ICommandAnswer> TopGirls(long chatId, bool reverse = false)
		{
			await using var db = new Database();

			var query = db.Girls.Where(x => x.ChatID == chatId);

			query = reverse ? query.OrderBy(x => x.Rating) : query.OrderByDescending(x => x.Rating);

			var topGirls = await query.Take(top_girls_amount).ToListAsync();
			if (topGirls.Count > 0)
			{
				var girlsToRemove = topGirls.Where(x => x.Rating < delete_rating_threshold).ToArray();
				if (girlsToRemove.Length > 0)
				{
					db.Girls.RemoveRange(girlsToRemove);
					await db.SaveChangesAsync();
				}

				// telegram ignores albums with one image for some reason
				if (topGirls.Count == 1)
					return new ImageCommandAnswer { Image = topGirls[0].Link, Caption = topGirls[0].Rating.ToString() };

				return new ImageAlbumCommandAnswer(topGirls.Select(girl => new InputMediaPhoto(girl.Link) { Caption = girl.Rating.ToString() }).ToList());
			}

			return Localization.GetAnswer("girls_not_found", chatId);
		}

		private async Task<ICommandAnswer> TopGirlsSeasonal(Message msg)
		{
			int season = DatabaseCache.GetGirlSeason();

			var split = msg.Text!.Split(' ');
			if (split.Length > 1
			    && int.TryParse(split[1], out var s)
			    && s <= season) // we dont want to spoil next season
			{
				season = s;
			}

			await using var db = new Database();

			var topGirls = await db.Girls
				.Where(x => x.ChatID == msg.Chat.Id && x.Season == season)
				.OrderByDescending(x => x.SeasonRating)
				.Take(top_girls_amount)
				.ToArrayAsync();

			if (topGirls.Length > 0)
			{
				var girlsToRemove = topGirls.Where(x => x.Rating < delete_rating_threshold).ToArray();
				if (girlsToRemove.Length > 0)
				{
					db.Girls.RemoveRange(girlsToRemove);
					await db.SaveChangesAsync();
				}

				// telegram ignores albums with one image for some reason
				if (topGirls.Length == 1)
					return new ImageCommandAnswer { Image = topGirls[0].Link, Caption = $"{topGirls[0].SeasonRating} (s{season})" };

				return new ImageAlbumCommandAnswer(topGirls.Select(girl => new InputMediaPhoto(girl.Link) { Caption = $"{girl.SeasonRating} (s{season})" }).ToList());
			}

			return Localization.GetAnswer("girls_not_found", msg.Chat.Id);
		}

		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Message is null || callback.Data != "+" && callback.Data != "-") // sanity check
				return string.Empty;

			var chatId = callback.Message.Chat.Id;

			if (!sentGirlsCache.Contains(callback.Message.MessageId.ToString()))
			{
				// remove buttons from outdated messages
				await API.EditMediaCaption(chatId, callback.Message.MessageId, callback.Message.Caption);
				return string.Empty;
			}

			if (sentGirlsCache.Get(callback.Message.MessageId.ToString()) is not SentGirl sentGirl)
				return string.Empty;

			if (sentGirl.Voters != null && sentGirl.Voters.Contains(callback.From.Id))
			{
				// they already voted
				return Localization.Get($"rating_repeat_{RNG.NextNoMemory(1, 10)}", chatId);
			}
			sentGirl.Voters?.Add(callback.From.Id);

			await using var db = new Database();

			var dbGirl = await db.Girls.FirstAsync(x=> x.Id == sentGirl.ID);

			switch (callback.Data)
			{
				case "+":
				{
					if (sentGirl.Seasonal)
					{
						dbGirl.SeasonRating++;
						sentGirl.SeasonalRating++;
					}
					else
					{
						dbGirl.Rating++;
						sentGirl.Rating++;
					}

					var caption = sentGirl.Rating.ToString();
					if (sentGirl.Seasonal)
						caption = $"{sentGirl.SeasonalRating} (s{sentGirl.Season})";
					
					await API.EditMediaCaption(chatId, callback.Message.MessageId, caption, buttons);

					await db.SaveChangesAsync();
					return Localization.FormatGet("girls_rating_up", chatId, caption);
				}
				case "-":
				{
					if (sentGirl.Seasonal)
					{
						dbGirl.SeasonRating--;
						sentGirl.SeasonalRating--;
					}
					else
					{
						dbGirl.Rating--;
						sentGirl.Rating--;
					}

					if (sentGirl.Rating < delete_rating_threshold || sentGirl.SeasonalRating < delete_rating_threshold)
					{
						// remove girls with rating below delete_rating_threshold
						sentGirlsCache.Remove(callback.Message.MessageId.ToString());
						db.Girls.Remove(dbGirl);
						await db.SaveChangesAsync();

						await API.RemoveMessage(chatId, callback.Message.MessageId);
						return Localization.Get("girls_rating_delete_lowrating", chatId);
					}

					var caption = sentGirl.Rating.ToString();
					if (sentGirl.Seasonal)
						caption = $"{sentGirl.SeasonalRating} (s{sentGirl.Season})";

					await API.EditMediaCaption(chatId, callback.Message.MessageId, caption, buttons);

					await db.SaveChangesAsync();
					return Localization.FormatGet("girls_rating_down", chatId, caption);
				}
				default:
					await db.SaveChangesAsync();

					return string.Empty;
			}
		}

		private async Task<ICommandAnswer> DeleteGirl(Message message)
		{
			if (message.ReplyToMessage == null || !sentGirlsCache.Contains(message.ReplyToMessage.MessageId.ToString())) 
				return null;

			await using var db = new Database();

			var girl = sentGirlsCache.Remove(message.ReplyToMessage.MessageId.ToString()) as SentGirl;

			db.Girls.Remove(db.Girls.First(x => x.Id == girl.ID));
			await db.SaveChangesAsync();

			await API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);

			return Localization.GetAnswer("girls_rating_delete_manual", message.Chat.Id);
		}

		private async Task<Girl> GetGirl(long chatId)
		{
			await using var db = new Database();

			Girl girl;

			var iteration = 0;
			while (iteration < max_reroll_iterations)
			{
				girl = await db.Girls
					.Where(x => x.ChatID == chatId)
					.OrderBy(r => EF.Functions.Random())
					.FirstOrDefaultAsync();

				if (girl is null)
					return null;

				if (sentGirlsCache.Any(x => ((SentGirl)x.Value).ID == girl.Id))
				{
					iteration++;
					continue;
				}

				return girl;
			}

			return null;
		}

		private async Task<Girl> GetGirlSeasonal(long chatId)
		{
			var season = DatabaseCache.GetGirlSeason();
			await using var db = new Database();

			Girl girl;

			var iteration = 0;
			while (iteration < max_reroll_iterations)
			{
				girl = await db.Girls
					.Where(x => x.ChatID == chatId && x.Season == season)
					.OrderBy(r => EF.Functions.Random())
					.FirstOrDefaultAsync();

				if (girl is null)
				{
					await PopulateFirstSeason(chatId);
					return null;
				}

				if (sentGirlsCache.Any(x => ((SentGirl)x.Value).ID == girl.Id))
				{
					iteration++;
					continue;
				}

				return girl;
			}

			return null;
		}

		private async Task PopulateFirstSeason(long chatId)
		{
			var season = DatabaseCache.GetGirlSeason();
			if (season == 1)
			{
				await using var db = new Database();

				// populate first season with latest girls
				var allGirls = await db.Girls.Where(x => x.ChatID == chatId).Reverse().Take(100).ToArrayAsync();
				foreach (var girl in allGirls)
				{
					girl.Season = 1;
					db.Girls.Update(girl);
				}
				await db.SaveChangesAsync();
			}
		}

		private async Task<ICommandAnswer> Migrate(Message msg)
		{
			var split = msg.Text!.Split(' ');
			if (split.Length > 1 && long.TryParse(split[1], out var id))
			{
				await using var db = new Database();

				var girlsToMigrate = await db.Girls.Where(x => x.ChatID == id).ToArrayAsync();
				foreach (var girl in girlsToMigrate)
				{
					girl.ChatID = msg.Chat.Id;
					db.Girls.Update(girl);
				}

				await db.SaveChangesAsync();
			}

			return Localization.GetAnswer("generic_fail", msg.Chat.Id);
		}
	}
}