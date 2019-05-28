// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using den0bot.DB;
using den0bot.Util;
using System.Threading;
using System.Linq;

namespace den0bot.Modules
{
	public class ModGirls : IModule, IReceiveAllMessages, IReceiveCallback, IReceivePhotos
	{
		private class ChachedGirl
		{
			public int ID { get; set; }
			public int Rating { get; set; }
			public List<int> Voters { get; set; }
			public DateTime PostTime { get; set; }
			public int MessageID { get; set; }
			public int CommandMessageID { get; set; }
			public bool Seasonal { get; set; }
			public int SeasonalRating { get; set; }
		}

		private readonly MemoryCache sentGirlsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep girls in cache

		private readonly InlineKeyboardMarkup buttons = new InlineKeyboardMarkup(
			new [] { new InlineKeyboardButton {Text = "+", CallbackData = "+" },
					 new InlineKeyboardButton {Text = "-", CallbackData = "-" },
			}
		);

		private readonly Dictionary<long, Queue<ChachedGirl>> antispamBuffer = new Dictionary<long, Queue<ChachedGirl>>(); // chatID, queue
		private const int antispam_cooldown = 15; //seconds

		private const int top_girls_amount = 9;
		private const int delete_rating_threshold = -10; // lowest rating a girl can have before completely removing her from db

		public ModGirls()
		{
			AddCommands(new[]
			{
				new Command
				{
					Names = { "devka", "tyanochku", "girl" },
					ActionAsync = msg => GetRandomGirl(msg),
				},
				new Command
				{
					Names = {"bestdevka", "bestgirl"},
					ActionAsync = (msg) => GetRandomPlatinumGirl(msg.Chat),
				},
				new Command
				{
					Names = {"topdevok", "topgirls" },
					Action = (msg) => TopGirls(msg.Chat.Id)
				},
				new Command
				{
					Names = {"antitopdevok", "antitopgirls" },
					Action = (msg) => TopGirls(msg.Chat.Id, true)
				},
				new Command
				{
					Name = "resetgirlrating",
					IsOwnerOnly = true,
					Action = m => {Database.RemoveRatings(); return string.Empty; }
				},
				new Command
				{
					Name = "delet",
					IsAdminOnly = true,
					Reply = true,
					Action = DeleteGirl
				},
				new Command
				{
					Names = { "seasonaldevka", "seasonaltyanochku", "seasonalgirl" },
					ActionAsync = msg => GetRandomGirl(msg, true),
				},
				new Command
				{
					Names = { "seasonaltopdevok", "seasonaltopgirls" },
					Action = TopGirlsSeasonal
				},
			});
			Log.Debug("Enabled");
		}
		public void ReceiveMessage(Message message)
		{
			if (message.Type == MessageType.Photo && message.Caption == Localization.Get("girls_tag", message.Chat.Id))
			{
				Database.AddGirl(message.Photo[0].FileId, message.Chat.Id);
			}
		}

		private async Task<string> GetRandomGirl(Message msg, bool seasonal = false)
		{
			long chatID = msg.Chat.Id;

			if (Database.GetGirlCount(chatID) <= 0)
				return Localization.Get("girls_not_found", chatID);

			if (!antispamBuffer.ContainsKey(chatID))
				antispamBuffer.Add(chatID, new Queue<ChachedGirl>(3));

			DB.Types.Girl picture = seasonal ? Database.GetGirlSeasonal(chatID) : Database.GetGirl(chatID);
			if (picture != null && picture.Link != string.Empty)
			{
				var sentMessage = await API.SendPhoto(picture.Link, 
					chatID, 
					seasonal ? $"{picture.SeasonRating} (s{Database.GirlSeason})" : picture.Rating.ToString(), 
					ParseMode.Default, 
					0, 
					buttons);

				if (sentMessage != null)
				{
					var girl = new ChachedGirl
					{
						ID = picture.Id,
						Rating = picture.Rating == int.MinValue ? 0 : picture.Rating,
						Voters = new List<int>(),
						PostTime = DateTime.Now,
						MessageID = sentMessage.MessageId,
						CommandMessageID = msg.MessageId,
						Seasonal = seasonal,
						SeasonalRating = picture.SeasonRating == int.MinValue ? 0 : picture.SeasonRating
					};
					sentGirlsCache.Add(sentMessage.MessageId.ToString(), girl,
						DateTimeOffset.Now.AddDays(days_to_keep_messages));
					antispamBuffer[chatID].Enqueue(girl);

					if (antispamBuffer[chatID].Count == 3)
					{
						// check if third girl in a queue was posted less than antispam_cooldown seconds ago and remove it
						var oldestGirl = antispamBuffer[chatID].Dequeue();
						var cd = oldestGirl.PostTime.AddSeconds(antispam_cooldown);
						if (cd > DateTime.Now)
						{
							sentGirlsCache.Remove(oldestGirl.MessageID.ToString());
							API.RemoveMessage(chatID, oldestGirl.MessageID);
							Thread.Sleep(50); // going too fast breaks api
							API.RemoveMessage(chatID, oldestGirl.CommandMessageID);
						}
					}
				}

				return string.Empty;
			}

			return Localization.Get("generic_fail", chatID);
		}
		private async Task<string> GetRandomPlatinumGirl(Chat sender)
		{
			DB.Types.Girl picture = Database.GetPlatinumGirl(sender.Id);
			if (picture != null && picture.Link != string.Empty)
			{
				await API.SendPhoto(picture.Link, sender.Id);
				return string.Empty;
			}

			return Localization.Get("girls_not_found", sender.Id);
		}
		private string TopGirls(long chatID, bool reverse = false)
		{
			var topGirls = Database.GetTopGirls(chatID);
			if (topGirls != null)
			{
				List<InputMediaPhoto> photos = new List<InputMediaPhoto>();

				// if we want the worst rated ones
				if (reverse)
					topGirls.Reverse();

				topGirls = topGirls.Take(top_girls_amount).ToList();
				foreach (var girl in topGirls)
				{
					if (girl.Rating < delete_rating_threshold)
						Database.RemoveGirl(girl.Id); // just in case

					photos.Add(new InputMediaPhoto(girl.Link) { Caption = girl.Rating.ToString() });
				}
				API.SendMultiplePhotos(photos, chatID).NoAwait();
			}
			return string.Empty;
		}
		private string TopGirlsSeasonal(Message msg)
		{
			int season = Database.GirlSeason;

			var split = msg.Text.Split(' ');
			if (split.Length > 1
			    && int.TryParse(msg.Text.Split(' ')[1], out var s)
			    && s <= season) // we dont want to spoil next season
			{
				season = s;
			}

			var topGirls = Database.GetTopGirlsSeasonal(msg.Chat.Id, season);
			if (topGirls != null)
			{
				List<InputMediaPhoto> photos = new List<InputMediaPhoto>();

				topGirls = topGirls.Take(top_girls_amount).ToList();
				foreach (var girl in topGirls)
				{
					if (girl.SeasonRating < delete_rating_threshold)
						Database.RemoveGirl(girl.Id); // just in case

					photos.Add(new InputMediaPhoto(girl.Link) { Caption = $"{girl.SeasonRating} (s{season})" });
				}
				API.SendMultiplePhotos(photos, msg.Chat.Id).NoAwait();
			}
			return string.Empty;
		}
		public void ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Data == "+" || callback.Data == "-") // sanity check
			{
				if (sentGirlsCache.Contains(callback.Message.MessageId.ToString()))
				{
					long chatId = callback.Message.Chat.Id;
					var girl = sentGirlsCache.Get(callback.Message.MessageId.ToString()) as ChachedGirl;
					if (girl.Voters != null && girl.Voters.Contains(callback.From.Id))
					{
						// they already voted
						API.AnswerCallbackQuery(callback.Id, Events.RatingRepeat(chatId));
					}
					else
					{
						if (callback.Data == "+")
						{
							if (girl.Seasonal)
							{
								Database.ChangeGirlRatingSeasonal(girl.ID, 1);
								girl.SeasonalRating++;

								API.EditMediaCaption(chatId, callback.Message.MessageId, $"{girl.SeasonalRating} (s{Database.GirlSeason})",
									buttons);
								API.AnswerCallbackQuery(callback.Id, Localization.FormatGet("girls_rating_up", $"{girl.SeasonalRating} (s{Database.GirlSeason})", chatId));
							}
							else
							{
								Database.ChangeGirlRating(girl.ID, 1);
								girl.Rating++;

								API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(),
									buttons);
								API.AnswerCallbackQuery(callback.Id, Localization.FormatGet("girls_rating_up", girl.Rating, chatId));
							}

							girl.Voters.Add(callback.From.Id);
							
						}
						else if (callback.Data == "-")
						{
							if (girl.Seasonal)
							{
								Database.ChangeGirlRatingSeasonal(girl.ID, -1);
								girl.SeasonalRating--;
							}
							else
							{
								Database.ChangeGirlRating(girl.ID, -1);
								girl.Rating--;
							}
							girl.Voters.Add(callback.From.Id);

							if (girl.Rating >= delete_rating_threshold && girl.SeasonalRating >= delete_rating_threshold)
							{
								if (girl.Seasonal)
								{
									API.EditMediaCaption(chatId, callback.Message.MessageId, $"{girl.SeasonalRating} (s{Database.GirlSeason})",
										buttons);
									API.AnswerCallbackQuery(callback.Id,
										Localization.FormatGet("girls_rating_down", $"{girl.SeasonalRating} (s{Database.GirlSeason})", chatId));
								}
								else
								{
									API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(),
										buttons);
									API.AnswerCallbackQuery(callback.Id,
										Localization.FormatGet("girls_rating_down", girl.Rating, chatId));
								}
							}
							else
							{
								sentGirlsCache.Remove(callback.Message.MessageId.ToString());
								Database.RemoveGirl(girl.ID);
								API.RemoveMessage(chatId, callback.Message.MessageId);
								API.AnswerCallbackQuery(callback.Id, Localization.Get("girls_rating_delete_lowrating", chatId));
							}
						}
					}
				}
				else
				{
					// remove buttons from outdated messages
					API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, callback.Message.Caption);
				}
			}
		}

		private string DeleteGirl(Message message)
		{
			if (message.ReplyToMessage != null && sentGirlsCache.Contains(message.ReplyToMessage.MessageId.ToString()))
			{
				var girl = sentGirlsCache.Remove(message.ReplyToMessage.MessageId.ToString()) as ChachedGirl;
				Database.RemoveGirl(girl.ID);
				API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);
				return Localization.Get("girls_rating_delete_manual", message.Chat.Id);
			}
			return string.Empty;
		}
	}
}