﻿// den0bot (c) StanR 2021 - MIT License
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
			public List<int> Voters { get; init; }
			public DateTime PostTime { get; init; }
			public int MessageID { get; init; }
			public int CommandMessageID { get; init; }
			public bool Seasonal { get; init; }
			public int Season { get; init; }
			public int SeasonalRating { get; set; }
		}

		private readonly MemoryCache sentGirlsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep girls in cache

		private readonly InlineKeyboardMarkup buttons = new(
			new [] { new InlineKeyboardButton {Text = "+", CallbackData = "+" },
					 new InlineKeyboardButton {Text = "-", CallbackData = "-" },
			}
		);

		private readonly Dictionary<long, Queue<SentGirl>> antispamBuffer = new(); // chatID, queue
		private const int antispam_cooldown = 15; // seconds
		private const int antispam_buffer_capacity = 4;

		private const int top_girls_amount = 9;
		private const int delete_rating_threshold = -10; // lowest rating a girl can have before completely removing her from db
		private const int cooldown_in_days = 1; // how many days a girl shouldn't appear again

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
				}
			});
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
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
			long chatID = msg.Chat.Id;
			await using var db = new Database();

			if (await db.Girls.CountAsync(x=> x.ChatID == chatID) <= 0)
				return Localization.GetAnswer("girls_not_found", chatID);

			if (!antispamBuffer.ContainsKey(chatID))
				antispamBuffer.Add(chatID, new Queue<SentGirl>(antispam_buffer_capacity));

			var picture = seasonal ? await GetGirlSeasonal(chatID) : await GetGirl(chatID);
			if (picture != null && picture.Link != string.Empty)
			{
				var sentMessage = await API.SendPhoto(picture.Link, 
					chatID, 
					seasonal ? $"{picture.SeasonRating} (s{picture.Season})" : picture.Rating.ToString(), 
					ParseMode.Default, 
					0, 
					buttons,
					false);

				if (sentMessage != null)
				{
					var girl = new SentGirl
					{
						ID = picture.Id,
						Rating = picture.Rating == int.MinValue ? 0 : picture.Rating,
						Voters = new List<int>(),
						PostTime = DateTime.Now,
						MessageID = sentMessage.MessageId,
						CommandMessageID = msg.MessageId,
						Seasonal = seasonal,
						Season = DatabaseCache.GetGirlSeason(),
						SeasonalRating = picture.SeasonRating == int.MinValue ? 0 : picture.SeasonRating
					};
					sentGirlsCache.Add(sentMessage.MessageId.ToString(), girl, DateTimeOffset.Now.AddDays(days_to_keep_messages));
					antispamBuffer[chatID].Enqueue(girl);

					if (antispamBuffer[chatID].Count == antispam_buffer_capacity)
					{
						// check if third girl in a queue was posted less than antispam_cooldown seconds ago and remove it
						var oldestGirl = antispamBuffer[chatID].Dequeue();
						var cd = oldestGirl.PostTime.AddSeconds(antispam_cooldown);
						if (cd > DateTime.Now)
						{
							sentGirlsCache.Remove(oldestGirl.MessageID.ToString());
							await API.RemoveMessage(chatID, oldestGirl.MessageID);
							await API.RemoveMessage(chatID, oldestGirl.CommandMessageID);
						}
					}
				}

				return null;
			}

			return Localization.GetAnswer("generic_fail", chatID);
		}

		private async Task<ICommandAnswer> GetRandomPlatinumGirl(Telegram.Bot.Types.Chat sender)
		{
			await using var db = new Database();

			var girl = await RNG.DatabaseRandom(db.Girls.Where(x => x.ChatID == sender.Id && x.Rating >= 10));
			if (girl != null && girl.Link != string.Empty)
			{
				return new ImageCommandAnswer
				{ 
					Image = girl.Link,
					SendTextIfFailed = false
				};
			}
			return Localization.GetAnswer("girls_not_found", sender.Id);
		}

		private async Task<ICommandAnswer> TopGirls(long chatID, bool reverse = false)
		{
			await using var db = new Database();

			var query = db.Girls.Where(x => x.ChatID == chatID);

			// if we want the worst rated ones
			query = reverse ? query.OrderBy(x => x.Rating) : query.OrderByDescending(x => x.Rating);
			
			var topGirls = await query.Take(top_girls_amount).ToArrayAsync();
			if (topGirls != null)
			{
				if (topGirls.Length == 0)
					return Localization.GetAnswer("girls_not_found", chatID);

				List<InputMediaPhoto> photos = new List<InputMediaPhoto>();
				foreach (var girl in topGirls)
				{
					if (girl.Rating < delete_rating_threshold)
					{
						// just in case
						db.Girls.Remove(girl);
						await db.SaveChangesAsync();
					}

					photos.Add(new InputMediaPhoto(girl.Link) {Caption = girl.Rating.ToString()});
				}

				return new ImageAlbumCommandAnswer(photos);
			}

			return Localization.GetAnswer("generic_fail", chatID);
		}

		private async Task<ICommandAnswer> TopGirlsSeasonal(Message msg)
		{
			int season = DatabaseCache.GetGirlSeason();

			var split = msg.Text.Split(' ');
			if (split.Length > 1
			    && int.TryParse(msg.Text.Split(' ')[1], out var s)
			    && s <= season) // we dont want to spoil next season
			{
				season = s;
			}

			await using var db = new Database();

			var topGirls = await db.Girls
				.Where(x => x.ChatID == msg.Chat.Id && x.Season == season)
				.Take(top_girls_amount)
				.OrderByDescending(x => x.SeasonRating)
				.ToArrayAsync();

			if (topGirls.Length <= 0)
				return Localization.GetAnswer("generic_fail", msg.Chat.Id);

			List<InputMediaPhoto> photos = new List<InputMediaPhoto>();

			foreach (var girl in topGirls)
			{
				if (girl.SeasonRating < delete_rating_threshold)
				{
					// just in case
					db.Girls.Remove(girl);
					await db.SaveChangesAsync();
				}

				photos.Add(new InputMediaPhoto(girl.Link) {Caption = $"{girl.SeasonRating} (s{season})"});
			}

			return new ImageAlbumCommandAnswer(photos);
		}

		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Data != "+" && callback.Data != "-") // sanity check
				return string.Empty;

			if (!sentGirlsCache.Contains(callback.Message.MessageId.ToString()))
			{
				// remove buttons from outdated messages
				await API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, callback.Message.Caption);
				return string.Empty;
			}

			long chatId = callback.Message.Chat.Id;
			var sentGirl = sentGirlsCache.Get(callback.Message.MessageId.ToString()) as SentGirl;
			if (sentGirl?.Voters != null && sentGirl.Voters.Contains(callback.From.Id))
			{
				// they already voted
				return Localization.Get($"rating_repeat_{RNG.NextNoMemory(1, 10)}", chatId);
			}

			await using var db = new Database();

			var dbGirl = await db.Girls.FirstOrDefaultAsync(x=> x.Id == sentGirl.ID);
			if (dbGirl == null || sentGirl == null)
				return string.Empty;

			if (callback.Data == "+")
			{
				sentGirl.Voters?.Add(callback.From.Id);

				var caption = string.Empty;
				if (sentGirl.Seasonal)
				{
					dbGirl.SeasonRating++;
					sentGirl.SeasonalRating++;
					caption = $"{sentGirl.SeasonalRating} (s{sentGirl.Season})";
				}
				else
				{
					dbGirl.Rating++;
					sentGirl.Rating++;
					caption = sentGirl.Rating.ToString();
				}
				await db.SaveChangesAsync();

				await API.EditMediaCaption(chatId, callback.Message.MessageId, caption, buttons);
				return Localization.FormatGet("girls_rating_up", chatId, caption);
			}

			if (callback.Data == "-")
			{
				sentGirl.Voters?.Add(callback.From.Id);

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

				await db.SaveChangesAsync();

				await API.EditMediaCaption(chatId, callback.Message.MessageId, caption, buttons);
				return Localization.FormatGet("girls_rating_down", chatId, caption);
			}

			return string.Empty;
		}

		private async Task<ICommandAnswer> DeleteGirl(Message message)
		{
			if (message.ReplyToMessage != null && sentGirlsCache.Contains(message.ReplyToMessage.MessageId.ToString()))
			{
				await using var db = new Database();

				var girl = sentGirlsCache.Remove(message.ReplyToMessage.MessageId.ToString()) as SentGirl;

				db.Girls.Remove(db.Girls.First(x => x.Id == girl.ID));
				await db.SaveChangesAsync();

				await API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);

				return Localization.GetAnswer("girls_rating_delete_manual", message.Chat.Id);
			}
			return null;
		}

		private async Task<Girl> GetGirl(long chatId)
		{
			await using var db = new Database();

			var cooldownTime = DateTime.Now.AddDays(-cooldown_in_days);
			Girl girl;

			var query = db.Girls.Where(x => x.ChatID == chatId);
			var unusedGirlsAmount = await query.Where(x=> x.LastUsed < cooldownTime).CountAsync();
			if (unusedGirlsAmount > 0)
			{
				girl = await RNG.DatabaseRandom(query.Where(x => x.LastUsed < cooldownTime), unusedGirlsAmount);
			}
			else
			{
				// if all girls were used return any random girl
				girl = await RNG.DatabaseRandom(query);
			}

			girl.LastUsed = DateTime.Now;
			db.Update(girl);

			await db.SaveChangesAsync();

			return girl;
		}

		private async Task<Girl> GetGirlSeasonal(long chatId)
		{
			await using var db = new Database();

			var season = DatabaseCache.GetGirlSeason();
			var cooldownTime = DateTime.Now.AddDays(-cooldown_in_days);
			Girl girl;

			var query = db.Girls.Where(x => x.ChatID == chatId && x.Season == season);
			var unusedGirlsAmount = await query.Where(x => x.SeasonLastUsed < cooldownTime).CountAsync();
			if (unusedGirlsAmount > 0)
			{
				girl = await RNG.DatabaseRandom(query.Where(x => x.SeasonLastUsed < cooldownTime), unusedGirlsAmount);
			}
			else
			{
				// if all girls were used return any random girl
				girl = await RNG.DatabaseRandom(query);
			}

			girl.SeasonLastUsed = DateTime.Now;
			db.Update(girl);

			await db.SaveChangesAsync();

			return girl;
		}
	}
}