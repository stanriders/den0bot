// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using den0bot.DB;
using den0bot.Util;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
namespace den0bot.Modules
{
	class ChachedGirl
	{
		public int ID { get; set; }
		public int Rating { get; set; }
		public List<int> Voters { get; set; }
	}
    public class ModGirls : IModule, IReceiveAllMessages, IReceiveCallback, IReceivePhotos
	{
        private MemoryCache sentGirlsCache = MemoryCache.Default; // messageID, girlID

		private InlineKeyboardMarkup buttons = new InlineKeyboardMarkup(
			new InlineKeyboardButton[] {
									new InlineKeyboardButton() {Text = "+", CallbackData = "+" },
									new InlineKeyboardButton() {Text = "-", CallbackData = "-" },
			}
		);

		public ModGirls()
        {
			AddCommands(new Command[]
			{
				new Command
				{
					Name = "devka",
					ActionAsync = (msg) => GetRandomGirl(msg.Chat),
				},
				new Command
				{
					Name = "bestdevka",
					ActionAsync = (msg) => GetRandomPlatinumGirl(msg.Chat),
				},
				new Command
				{
					Name = "topdevok",
					Action = (msg) => TopGirls(msg.Chat.Id)
				},
				new Command
				{
					Name = "antitopdevok",
					Action = (msg) => TopGirls(msg.Chat.Id, true)
				},
				new Command
				{
					Name = "resetgirlrating",
					IsOwnerOnly = true,
					Action = (msg) => {Database.RemoveRatings(); return string.Empty; }
				},
				new Command
				{
					Name = "delet",
					IsAdminOnly = true,
					Reply = true,
					Action = (msg) => DeleteGirl(msg)
				}
            });
            Log.Info(this, "Enabled");
        }
        public void ReceiveMessage(Message message)
        {
            if (message.Type == MessageType.Photo && message.Caption == Localization.Get("girls_tag", message.Chat.Id))
            {
                Database.AddGirl(message.Photo[0].FileId, message.Chat.Id);
            }
        }

        private async Task<string> GetRandomGirl(Chat sender)
        {
            int girlCount = Database.GetGirlCount(sender.Id);
            if (girlCount <= 0)
                return Localization.Get("girls_not_found", sender.Id);

            DB.Types.Girl picture = Database.GetGirl(sender.Id);
            if (picture != null && picture.Link != string.Empty)
            {
				var message = await API.SendPhoto(picture.Link, sender.Id, picture.Rating.ToString(), ParseMode.Default, 0, buttons);
				var girl = new ChachedGirl()
				{
					ID = picture.Id,
					Rating = picture.Rating == int.MinValue ? 0 : picture.Rating,
					Voters = new List<int>()
				};
				sentGirlsCache.Add(message.MessageId.ToString(), girl, DateTimeOffset.Now.AddDays(1));

                return string.Empty;
            }

            return Localization.Get("generic_fail", sender.Id); 

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

                if (reverse)
                    topGirls.Reverse();

                if (topGirls.Count > 9)
                    topGirls.RemoveRange(9, topGirls.Count - 9); // keep only top10 

                foreach (var girl in topGirls)
                {
                    if (girl.Rating < -10)
                        Database.RemoveGirl(girl.Id); // just in case

                    photos.Add(new InputMediaPhoto(girl.Link) { Caption = girl.Rating.ToString() });
                }
                API.SendMultiplePhotos(photos, chatID);
            }
            return string.Empty;
        }

		public void ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Data == "+" || callback.Data == "-")
			{
				if (sentGirlsCache.Contains(callback.Message.MessageId.ToString()))
				{
					long chatId = callback.Message.Chat.Id;
					var girl = sentGirlsCache.Get(callback.Message.MessageId.ToString()) as ChachedGirl;
					if (girl.Voters != null && girl.Voters.Contains(callback.From.Id))
					{
						// they already voted
						API.AnswerCallbackQuery(callback.Id, Events.RatingRepeat());
					}
					else
					{
						if (callback.Data == "+")
						{
							Database.ChangeGirlRating(girl.ID, 1);
							girl.Voters.Add(callback.From.Id);
							girl.Rating++;

							API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(), buttons);
							API.AnswerCallbackQuery(callback.Id, string.Format(Localization.Get("girls_rating_up", chatId), girl.Rating));//$"Рейтинг девки повышен ({girl.Rating})");
						}
						else if (callback.Data == "-")
						{
							Database.ChangeGirlRating(girl.ID, -1);
							girl.Voters.Add(callback.From.Id);
							girl.Rating--;
							if (girl.Rating >= -10)
							{
								API.EditMediaCaption(chatId, callback.Message.MessageId, girl.Rating.ToString(), buttons);
								API.AnswerCallbackQuery(callback.Id, string.Format(Localization.Get("girls_rating_down", chatId), girl.Rating));// $"Рейтинг девки понижен ({})");
							}
							else
							{
								sentGirlsCache.Remove(callback.Message.MessageId.ToString());
								Database.RemoveGirl(girl.ID);
								API.RemoveMessage(chatId, callback.Message.MessageId);
								API.AnswerCallbackQuery(callback.Id, Localization.Get("girls_rating_delete_lowrating", chatId));// "Девка удалена (рейтинг ниже -10)");
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
				var girl = sentGirlsCache.Get(message.ReplyToMessage.MessageId.ToString()) as ChachedGirl;
				sentGirlsCache.Remove(message.ReplyToMessage.MessageId.ToString());

				Database.RemoveGirl(girl.ID);
				API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);
				return Localization.Get("girls_rating_delete_manual", message.Chat.Id);
			}
			return string.Empty;
		}
	}
}

#pragma warning restore CS4014