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
using System.Linq;
using SQLite;

namespace den0bot.Modules
{
	public class ModGirls : IModule, IReceiveAllMessages, IReceiveCallback, IReceivePhotos
	{
		private class CachedGirl
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

		private readonly Dictionary<long, Queue<CachedGirl>> antispamBuffer = new Dictionary<long, Queue<CachedGirl>>(); // chatID, queue
		private const int antispam_cooldown = 15; //seconds

		private const int top_girls_amount = 9;
		private const int delete_rating_threshold = -10; // lowest rating a girl can have before completely removing her from db

		public ModGirls()
		{
			Database.CreateTable<Girl>();

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
					ActionAsync = (msg) => TopGirls(msg.Chat.Id)
				},
				new Command
				{
					Names = {"antitopdevok", "antitopgirls" },
					ActionAsync = (msg) => TopGirls(msg.Chat.Id, true)
				},
				new Command
				{
					Name = "resetgirlrating",
					IsOwnerOnly = true,
					Action = m => {RemoveRatings(); return string.Empty; }
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
			});
			Log.Debug("Enabled");
		}
		public Task ReceiveMessage(Message message)
		{
			if (message.Type == MessageType.Photo && message.Caption == Localization.Get("girls_tag", message.Chat.Id))
			{
				AddGirl(message.Photo[0].FileId, message.Chat.Id);
			}

			return Task.CompletedTask;
		}

		private async Task<string> GetRandomGirl(Message msg, bool seasonal = false)
		{
			long chatID = msg.Chat.Id;

			if (GetGirlCount(chatID) <= 0)
				return Localization.Get("girls_not_found", chatID);

			if (!antispamBuffer.ContainsKey(chatID))
				antispamBuffer.Add(chatID, new Queue<CachedGirl>(3));

			var picture = seasonal ? GetGirlSeasonal(chatID) : GetGirl(chatID);
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
					var girl = new CachedGirl
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
							await API.RemoveMessage(chatID, oldestGirl.MessageID);
							await API.RemoveMessage(chatID, oldestGirl.CommandMessageID);
						}
					}
				}

				return string.Empty;
			}

			return Localization.Get("generic_fail", chatID);
		}
		private async Task<string> GetRandomPlatinumGirl(Chat sender)
		{
			var picture = GetPlatinumGirl(sender.Id);
			if (picture != null && picture.Link != string.Empty)
			{
				await API.SendPhoto(picture.Link, sender.Id);
				return string.Empty;
			}

			return Localization.Get("girls_not_found", sender.Id);
		}
		private async Task<string> TopGirls(long chatID, bool reverse = false)
		{
			var topGirls = GetTopGirls(chatID);
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
						RemoveGirl(girl.Id); // just in case

					photos.Add(new InputMediaPhoto(girl.Link) { Caption = girl.Rating.ToString() });
				}
				await API.SendMultiplePhotos(photos, chatID);
			}
			return string.Empty;
		}
		private async Task<string> TopGirlsSeasonal(Message msg)
		{
			int season = Database.GirlSeason;

			var split = msg.Text.Split(' ');
			if (split.Length > 1
			    && int.TryParse(msg.Text.Split(' ')[1], out var s)
			    && s <= season) // we dont want to spoil next season
			{
				season = s;
			}

			var topGirls = GetTopGirlsSeasonal(msg.Chat.Id, season);
			if (topGirls != null)
			{
				List<InputMediaPhoto> photos = new List<InputMediaPhoto>();

				topGirls = topGirls.Take(top_girls_amount).ToList();
				foreach (var girl in topGirls)
				{
					if (girl.SeasonRating < delete_rating_threshold)
						RemoveGirl(girl.Id); // just in case

					photos.Add(new InputMediaPhoto(girl.Link) { Caption = $"{girl.SeasonRating} (s{season})" });
				}
				await API.SendMultiplePhotos(photos, msg.Chat.Id);
			}
			return string.Empty;
		}
		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Data == "+" || callback.Data == "-") // sanity check
			{
				if (sentGirlsCache.Contains(callback.Message.MessageId.ToString()))
				{
					long chatId = callback.Message.Chat.Id;
					var girl = sentGirlsCache.Get(callback.Message.MessageId.ToString()) as CachedGirl;
					if (girl?.Voters != null && girl.Voters.Contains(callback.From.Id))
					{
						// they already voted
						return Localization.Get($"rating_repeat_{RNG.NextNoMemory(1, 10)}", chatId);
					}
					else
					{
						if (callback.Data == "+")
						{
							girl.Voters?.Add(callback.From.Id);
							if (girl.Seasonal)
							{
								ChangeGirlRatingSeasonal(girl.ID, 1);
								girl.SeasonalRating++;

								await API.EditMediaCaption(chatId, callback.Message.MessageId, $"{girl.SeasonalRating} (s{Database.GirlSeason})",
									buttons);
								return Localization.FormatGet("girls_rating_up", chatId, $"{girl.SeasonalRating} (s{Database.GirlSeason})");
							}
							else
							{
								ChangeGirlRating(girl.ID, 1);
								girl.Rating++;

								await API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(),
									buttons);
								return Localization.FormatGet("girls_rating_up", chatId, girl.Rating);
							}	
						}
						else if (callback.Data == "-")
						{
							if (girl.Seasonal)
							{
								ChangeGirlRatingSeasonal(girl.ID, -1);
								girl.SeasonalRating--;
							}
							else
							{
								ChangeGirlRating(girl.ID, -1);
								girl.Rating--;
							}
							girl.Voters?.Add(callback.From.Id);

							if (girl.Rating >= delete_rating_threshold && girl.SeasonalRating >= delete_rating_threshold)
							{
								if (girl.Seasonal)
								{
									await API.EditMediaCaption(chatId, callback.Message.MessageId, $"{girl.SeasonalRating} (s{Database.GirlSeason})",
										buttons);
									return Localization.FormatGet("girls_rating_down", chatId, $"{girl.SeasonalRating} (s{Database.GirlSeason})");
								}
								else
								{
									await API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(),
										buttons);
									return Localization.FormatGet("girls_rating_down", chatId, girl.Rating);
								}
							}
							else
							{
								sentGirlsCache.Remove(callback.Message.MessageId.ToString());
								RemoveGirl(girl.ID);
								await API.RemoveMessage(chatId, callback.Message.MessageId);
								return Localization.Get("girls_rating_delete_lowrating", chatId);
							}
						}
					}
				}
				else
				{
					// remove buttons from outdated messages
					await API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, callback.Message.Caption);
				}
			}

			return string.Empty;
		}

		private async Task<string> DeleteGirl(Message message)
		{
			if (message.ReplyToMessage != null && sentGirlsCache.Contains(message.ReplyToMessage.MessageId.ToString()))
			{
				var girl = sentGirlsCache.Remove(message.ReplyToMessage.MessageId.ToString()) as CachedGirl;
				RemoveGirl(girl.ID);
				await API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);
				return Localization.Get("girls_rating_delete_manual", message.Chat.Id);
			}
			return string.Empty;
		}

		#region Database
		private class Girl
		{
			[PrimaryKey, AutoIncrement]
			public int Id { get; set; }

			public string Link { get; set; }

			public long ChatID { get; set; }

			public bool Used { get; set; }

			public int Rating { get; set; }

			// seasonal ratings
			public int Season { get; set; }

			public int SeasonRating { get; set; }

			public bool SeasonUsed { get; set; }
		}

		private int GetGirlCount(long chatID) => Database.Get<Girl>().Count(x => x.ChatID == chatID);
		private void AddGirl(string link, long chatID)
		{
			if (!Database.Exist<Girl>(x => x.Link == link))
			{
				var season = Database.GirlSeason;
				if (Database.GirlSeasonStartDate == default || Database.GirlSeasonStartDate.AddMonths(1) < DateTime.Today)
				{
					// rotate season if it's the day
					SubmitSeasonalRatingsToGlobal(season);
					Database.GirlSeason = ++season;
				}

				Database.Insert(new Girl
				{
					Link = link,
					ChatID = chatID,
					Rating = 0,
					Season = season + 1, // next season
					SeasonRating = 0
				});
			}
		}
		private void RemoveGirl(int id)
		{
			Database.Remove<Girl>(x => x.Id == id);
		}
		private Girl GetGirl(long chatID)
		{
			List<Girl> girls = Database.Get<Girl>(x => x.ChatID == chatID);
			if (girls != null)
			{
				girls.RemoveAll(x => x.Used == true);
				if (girls.Count == 0)
				{
					ResetUsedGirl(chatID);
					return GetGirl(chatID);
				}
				else
				{
					int num = RNG.Next(max: girls.Count);

					SetUsedGirl(girls[num].Id);
					return girls[num];
				}
			}
			return null;
		}
		private Girl GetPlatinumGirl(long chatID)
		{
			List<Girl> girls = Database.Get<Girl>(x => x.ChatID == chatID && x.Rating >= 10);
			if (girls != null && girls.Count > 0)
			{
				return girls[RNG.Next(max: girls.Count)];
			}
			return null;
		}
		private Girl GetGirlSeasonal(long chatID)
		{
			var season = Database.GirlSeason;
			if (Database.GirlSeasonStartDate == default || Database.GirlSeasonStartDate.AddMonths(1) < DateTime.Today)
			{
				// rotate season if it's the day
				SubmitSeasonalRatingsToGlobal(season);
				Database.GirlSeason = ++season;
			}

			List<Girl> girls = Database.Get<Girl>(x => x.ChatID == chatID && x.Season == season);
			if (girls != null)
			{
				if (girls.Count == 0)
				{
					if (Database.GirlSeason == 1)
					{
						// populate first season with latest girls
						List<Girl> allGirls = Database.Get<Girl>(x => x.ChatID == chatID);
						allGirls.Reverse();
						if (allGirls.Count > 100)
							allGirls = allGirls.Take(100).ToList();

						foreach (var girl in allGirls)
							girl.Season = 1;

						Database.UpdateAll(allGirls);
					}
					else
					{
						return null;
					}
				}

				girls.RemoveAll(x => x.SeasonUsed);
				if (girls.Count == 0)
				{
					ResetUsedGirlSeasonal(chatID);
					return GetGirlSeasonal(chatID);
				}
				else
				{
					int num = RNG.Next(max: girls.Count);

					SetUsedGirlSeasonal(girls[num].Id);
					return girls[num];
				}
			}
			return null;
		}
		private void SetUsedGirl(int id)
		{
			Girl girl = Database.GetFirst<Girl>(x => x.Id == id);
			if (girl != null)
			{
				girl.Used = true;
				Database.Update(girl);
			}
		}
		private void ResetUsedGirl(long chatID)
		{
			List<Girl> girls = Database.Get<Girl>(x => x.ChatID == chatID);
			foreach (Girl girl in girls)
				girl.Used = false;

			Database.UpdateAll(girls);
		}
		private void SetUsedGirlSeasonal(int id)
		{
			Girl girl = Database.GetFirst<Girl>(x => x.Id == id);
			if (girl != null)
			{
				girl.SeasonUsed = true;
				Database.Update(girl);
			}
		}
		private void ResetUsedGirlSeasonal(long chatID)
		{
			List<Girl> girls = Database.Get<Girl>(x => x.ChatID == chatID);
			foreach (Girl girl in girls)
				girl.SeasonUsed = false;

			Database.UpdateAll(girls);
		}
		private void ChangeGirlRating(int id, int rating)
		{
			Girl girl = Database.GetFirst<Girl>(x => x.Id == id);
			if (girl != null)
			{
				girl.Rating += rating;
				Database.Update(girl);
			}
		}
		private void ChangeGirlRatingSeasonal(int id, int rating)
		{
			Girl girl = Database.GetFirst<Girl>(x => x.Id == id);
			if (girl != null)
			{
				girl.SeasonRating += rating;
				Database.Update(girl);
			}
		}
		private List<Girl> GetTopGirls(long chatID)
		{
			return Database.Get<Girl>(x => x.ChatID == chatID)?.OrderByDescending(x => x.Rating)?.ToList();
		}
		private List<Girl> GetTopGirlsSeasonal(long chatID, int season)
		{
			return Database.Get<Girl>(x => x.ChatID == chatID && x.Season == season)?.OrderByDescending(x => x.SeasonRating)?.ToList();
		}
		private void RemoveRatings()
		{
			var table = Database.Get<Girl>();
			foreach (var girl in table)
			{
				girl.Rating = 0;
			}
			Database.UpdateAll(table);
		}
		private void SubmitSeasonalRatingsToGlobal(int season)
		{
			if (season > 0)
			{
				var table = Database.Get<Girl>(x => x.Season == season);
				foreach (var girl in table)
				{
					girl.Rating += girl.SeasonRating;
					if (girl.Rating < -10)
					{
						Database.Remove(girl);
						continue;
					}

					Database.Update(girl); // INEFFICIENT but works
				}
				Database.UpdateAll(table);
			}
		}
		#endregion
	}
}