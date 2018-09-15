// den0bot (c) StanR 2018 - MIT License
using den0bot.DB;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
namespace den0bot.Modules
{
    public class ModGirls : IModule, IProcessAllMessages, IReceiveCallback
	{
        public override bool NeedsPhotos => true;

        private Dictionary<int,int> sentGirls; // messageID, girlID
		//private readonly int sent_girls_to_keep = 128; // TODO

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
                    //ActionResult  = delegate (Message msg) { sentGirls.Add(msg); }
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
                }
            });
            sentGirls = new Dictionary<int, int>();
            Log.Info(this, "Enabled");
        }

        public void ReceiveMessage(Message message)
        {
            if (message.Type == MessageType.Photo && message.Caption == "#девки")
            {
                //foreach (var photo in message.Photo)
                {
                    Database.AddGirl(message.Photo[0].FileId, message.Chat.Id);
                }
            }
            else if (message.ReplyToMessage != null && sentGirls.ContainsKey(message.ReplyToMessage.MessageId))
            {
                var girlID = sentGirls[message.ReplyToMessage.MessageId];
                var girlVoters = Database.GetGirlVoters(girlID);
                if (message.Text == "+")
                {
                    if (girlVoters != null && girlVoters.Contains(message.From.Id))
                    {
						// already voted 
                        API.SendMessage(Events.RatingRepeat(), message.Chat, ParseMode.Default, message.MessageId);
                    }
                    else
                    {
                        Database.ChangeGirlRating(girlID, 1);
                        Database.AddGirlVoter(girlID, message.From.Id);
                        int rating = Database.GetGirlRating(girlID);
                        if (rating != int.MinValue && rating >= -10)
                        {
							//string caption = $"{message.ReplyToMessage.Caption}{Environment.NewLine}(+) {message.From.FirstName}";
							API.EditMediaCaption(message.Chat.Id, message.ReplyToMessage.MessageId, rating.ToString()/*caption*/, buttons);
						}
                    }
                    
                }
                else if (message.Text == "-")
                {
                    if (girlVoters != null && girlVoters.Contains(message.From.Id))
                    {
						// already voted
						API.SendMessage(Events.RatingRepeat(), message.Chat, ParseMode.Default, message.MessageId);
                    }
                    else
                    {
                        Database.ChangeGirlRating(girlID, -1);
                        Database.AddGirlVoter(girlID, message.From.Id);
                        int rating = Database.GetGirlRating(girlID);
                        if (rating != int.MinValue && rating >= -10)
                        {
							//string caption = $"{message.ReplyToMessage.Caption}{Environment.NewLine}(-) {message.From.FirstName}";
							API.EditMediaCaption(message.Chat.Id, message.ReplyToMessage.MessageId, rating.ToString()/*caption*/, buttons);
                        }
                        else
                        {
							Database.RemoveGirl(girlID);
							API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);
							API.SendMessage("Девка удалена (рейтинг ниже -10)", message.Chat, ParseMode.Default, message.MessageId);
                        }
                    }
                }
                else if (message.Text == "/delet")
                {
					if (Bot.IsAdmin(message.Chat.Id, message.From.Username))
					{
						Database.RemoveGirl(girlID);
						API.RemoveMessage(message.Chat.Id, message.ReplyToMessage.MessageId);
						API.SendMessage($"Девка удалена", message.Chat, ParseMode.Default, message.MessageId);
					}
                     
                }
            }
        }

        private async Task<string> GetRandomGirl(Chat sender)
        {
            int girlCount = Database.GetGirlCount(sender.Id);
            if (girlCount <= 0)
                return "А девок-то нет";

            DB.Types.Girl picture = Database.GetGirl(sender.Id);
            if (picture != null && picture.Link != string.Empty)
            {
                Database.ResetGirlVoters(picture.Id);

				var message = await API.SendPhoto(picture.Link, sender.Id, picture.Rating.ToString(), ParseMode.Default, 0, buttons);
                sentGirls.Add(message.MessageId, picture.Id);

                return string.Empty;
            }

            return "Чет не получилось";
        }
        private async Task<string> GetRandomPlatinumGirl(Chat sender)
        {
            DB.Types.Girl picture = Database.GetPlatinumGirl(sender.Id);
            if (picture != null && picture.Link != string.Empty)
            {
                //Database.ResetGirlVoters(picture.Id);
                await API.SendPhoto(picture.Link, sender.Id);
                // sentGirls.Add(message.MessageId, picture.Id);
                return string.Empty;
            }

            return "Нет девки";
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
			if (sentGirls.ContainsKey(callback.Message.MessageId))
			{
				var girlID = sentGirls[callback.Message.MessageId];
				var girlVoters = Database.GetGirlVoters(girlID);
				if (callback.Data == "+")
				{
					if (girlVoters != null && girlVoters.Contains(callback.From.Id))
					{
						// they already voted
						API.AnswerCallbackQuery(callback.Id, Events.RatingRepeat());
					}
					else
					{
						Database.ChangeGirlRating(girlID, 1);
						Database.AddGirlVoter(girlID, callback.From.Id);
						int rating = Database.GetGirlRating(girlID);
						if (rating != int.MinValue && rating >= -10)
						{
							//string caption = $"{callback.Message.Caption}{Environment.NewLine}(+) {callback.From.FirstName}";
							API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, rating.ToString()/*caption*/, buttons);
							API.AnswerCallbackQuery(callback.Id, $"Рейтинг девки повышен ({rating})");
						}
					}

				}
				else if (callback.Data == "-")
				{
					if (girlVoters != null && girlVoters.Contains(callback.From.Id))
					{
						// they already voted
						API.AnswerCallbackQuery(callback.Id, Events.RatingRepeat());
					}
					else
					{
						Database.ChangeGirlRating(girlID, -1);
						Database.AddGirlVoter(girlID, callback.From.Id);
						int rating = Database.GetGirlRating(girlID);
						if (rating != int.MinValue && rating >= -10)
						{
							//string caption = $"{callback.Message.Caption}{Environment.NewLine}(-) {callback.From.FirstName}";
							API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, rating.ToString()/*caption*/, buttons);
							API.AnswerCallbackQuery(callback.Id, $"Рейтинг девки понижен ({rating})");
						}
						else
						{
							Database.RemoveGirl(girlID);
							API.RemoveMessage(callback.Message.Chat.Id, callback.Message.MessageId);
							API.AnswerCallbackQuery(callback.Id, "Девка удалена (рейтинг ниже -10)");
						}
					}
				}
			}
			else if (callback.Data == "+" || callback.Data == "-")
			{
				// remove buttons from outdated messages
				API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, callback.Message.Caption);
			}
		}
	}
}

#pragma warning restore CS4014