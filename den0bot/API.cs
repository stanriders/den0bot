// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Util;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace den0bot
{
	static class API
	{
		public static User BotUser;
		private static TelegramBotClient api;

		public static bool IsConnected => api.IsReceiving;

		/// <summary>
		/// Connect and start receiving messages. Returns false if failed to connect.
		/// </summary>
		public static bool Connect()
		{
			if (string.IsNullOrEmpty(Config.Params.TelegamToken))
			{
				Log.Error("API", "Telegram token is null or empty!");
				return false;
			}

			Log.Info("API", "Connecting...");
			try
			{
				api = new TelegramBotClient(Config.Params.TelegamToken);
				api.OnMessage += OnMessage;
				api.OnCallbackQuery += OnCallback;
				api.OnReceiveGeneralError += delegate (object sender, ReceiveGeneralErrorEventArgs args) { Log.Error("API - OnReceiveGeneralError", args.Exception.InnerMessageIfAny()); };
				api.OnReceiveError += delegate (object sender, ReceiveErrorEventArgs args) { Log.Error("API - OnReceiveError", args.ApiRequestException.InnerMessageIfAny()); };

				if (!api.TestApiAsync().Result)
					return false;

				BotUser = api.GetMeAsync().Result;

				api.StartReceiving();
			}
			catch (Exception ex)
			{
				Log.Error("API", ex.InnerMessageIfAny());
				return false;
			}

			Log.Info("API", "Connected!");

			return true;
		}

		public static void Disconnect()
		{
			Log.Info("API", "Disconnecting...");
			api.StopReceiving();
		}

		public static event EventHandler<MessageEventArgs> OnMessage;
		public static event EventHandler<CallbackQueryEventArgs> OnCallback;

		/// <summary>
		/// Send message
		/// </summary>
		/// <param name="message">Text to send</param>
		/// <param name="receiver">Chat to send message to</param>
		/// <param name="mode">ParseMode to use (None/Markdown/HTML)</param>
		/// <param name="replyID">Message ID to reply to</param>
		public static void SendMessage(string message, Chat receiver, ParseMode mode = ParseMode.Default, int replyID = 0, IReplyMarkup replyMarkup = null, bool disablePreview = true) =>
			SendMessage(message, receiver.Id, mode, replyID, replyMarkup, disablePreview).NoAwait();

		/// <summary>
		/// Send message
		/// </summary>
		/// <param name="message">Text to send</param>
		/// <param name="receiverID">Chat ID to send message to</param>
		/// <param name="mode">ParseMode to use (None/Markdown/HTML)</param>
		/// <param name="replyID">Message ID to reply to</param>
		public static async Task<Message> SendMessage(string message, long receiverID, ParseMode mode = ParseMode.Default, int replyID = 0, IReplyMarkup replyMarkup = null, bool disablePreview = true)
		{
			try
			{
				if (!string.IsNullOrEmpty(message))
					return await api?.SendTextMessageAsync(receiverID, message, mode, disablePreview, false, replyID, replyMarkup);

				return null;
			}
			catch (Exception ex) { Log.Error("API - SendMessage", ex.InnerMessageIfAny()); return null; }
		}

		/// <summary>
		/// Send message to all chats in the database
		/// </summary>
		/// <param name="msg">Text to send</param>
		/// <param name="image">Image to send if any</param>
		/// <param name="mode">ParseMode to use (None/Markdown/HTML)</param>
		public static void SendMessageToAllChats(string msg, string image = null, ParseMode mode = ParseMode.Default)
		{
			foreach (DB.Types.Chat receiver in Database.GetAllChats())
			{
				if (api.GetChatAsync(receiver.Id).Result == null)
				{
					Database.RemoveChat(receiver.Id);
					Log.Info("API", $"Chat {receiver.Id} removed");
					return;
				}
				else if (!receiver.DisableAnnouncements)
				{
					if (!string.IsNullOrEmpty(image))
						SendPhoto(image, receiver.Id, msg, mode).NoAwait();
					else
						SendMessage(msg, receiver.Id, mode).NoAwait();
				}
			}
		}

		/// <summary>
		/// Send photo
		/// </summary>
		/// <param name="photo">Photo to send. Can be both internal telegram photo ID or a link</param>
		/// <param name="receiver">Chat to send photo to</param>
		/// <param name="message">Photo caption if any</param>
		public static void SendPhoto(string photo, Chat receiver, string message = "", ParseMode mode = ParseMode.Default, int replyID = 0, IReplyMarkup replyMarkup = null) =>
			SendPhoto(photo, receiver.Id, message, mode, replyID, replyMarkup).NoAwait();

		/// <summary>
		/// Send photo
		/// </summary>
		/// <param name="photo">Photo to send. Can be both internal telegram photo ID or a link</param>
		/// <param name="receiverId">Chat ID to send photo to</param>
		/// <param name="message">Photo caption if any</param>
		public static async Task<Message> SendPhoto(string photo, long receiverId, string message = "", ParseMode mode = ParseMode.Default, int replyID = 0, IReplyMarkup replyMarkup = null)
		{
			try
			{
				if (!string.IsNullOrEmpty(photo))
				{
					InputOnlineFile file = new InputOnlineFile(photo);
					if (photo.StartsWith("http") && (photo.EndsWith(".jpg") || photo.EndsWith(".png")))
					{
						file = UriPhotoDownload(new Uri(photo));
						if (file == null)
						{
							return await SendMessage(message, receiverId, mode, replyID, replyMarkup);
						}
					}

					return await api?.SendPhotoAsync(receiverId, file, message, mode, false, replyID, replyMarkup);
				}
				return null;
			}
			catch (Exception ex) { Log.Error("API - SendPhoto", ex.InnerMessageIfAny()); return null; }
		}
		private static InputOnlineFile UriPhotoDownload(Uri link)
		{
			try
			{
				return new InputOnlineFile(new WebClient().OpenRead(link));
			}
			catch (Exception ex)
			{
				Log.Error("API - UriPhotoDownload", ex.InnerMessageIfAny());
				return null;
			}
		}

		/// <summary>
		/// Send multiple photos in an album
		/// </summary>
		/// <param name="photos">Array of InputMediaPhoto photos</param>
		/// <param name="receiverID">Chat to send photos to</param>
		public static async Task<Message[]> SendMultiplePhotos(List<InputMediaPhoto> photos, long receiverId)
		{
			try
			{
				if (photos != null && photos.Count > 1)
				{
					return await api?.SendMediaGroupAsync(photos, receiverId);
				}
				return null;
			}
			catch (Exception ex) { Log.Error("API - SendMultiplePhotos", ex.InnerMessageIfAny()); return null; }
		}

		/// <summary>
		/// Send sticker
		/// </summary>
		/// <param name="sticker">Telegram sticker ID</param>
		/// <param name="receiverID">Chat to send sticker to</param>
		public static void SendSticker(InputOnlineFile sticker, long receiverID)
		{
			try
			{
				api.SendStickerAsync(receiverID, sticker);
			}
			catch (Exception ex) { Log.Error("API - SendSticker", ex.InnerMessageIfAny()); }
		}

		/// <summary>
		/// Returns List of chat memebers that are admins
		/// </summary>
		public static List<ChatMember> GetAdmins(long chatID)
		{
			return api?.GetChatAdministratorsAsync(chatID).Result?.ToList();
		}

		/// <summary>
		/// Remove message from a chat if bot have enough rights
		/// </summary>
		/// <param name="chatID">Chat ID to remove message from</param>
		/// <param name="msgID">Message to remove</param>
		public static void RemoveMessage(long chatID, int msgID)
		{
			try
			{
				api.DeleteMessageAsync(chatID, msgID);
			}
			catch (Exception ex) { Log.Error("API - RemoveMessage", ex.InnerMessageIfAny()); }
		}

		/// <summary>
		/// Remove media caption
		/// </summary>
		/// <param name="chatID">Chat ID to edit message in</param>
		/// <param name="msgID">Message to edit</param>
		/// <param name="caption">New caption</param>
		public static void EditMediaCaption(long chatID, int msgID, string caption, InlineKeyboardMarkup replyMarkup = null)
		{
			try
			{
				api.EditMessageCaptionAsync(chatID, msgID, caption, replyMarkup);
			}
			catch (Exception ex) { Log.Error("API - EditMediaCaption", ex.InnerMessageIfAny()); }
		}

		/// <summary>
		/// Answer callback query after receiving it
		/// </summary>
		/// <param name="callbackID">Callback ID that we need to answer</param>
		/// <param name="text">Text to send</param>
		/// <param name="showAlert">Alert user</param>
		public static void AnswerCallbackQuery(string callbackID, string text = null, bool showAlert = false)
		{
			try
			{
				api.AnswerCallbackQueryAsync(callbackID, text, showAlert);
			}
			catch (Exception ex) { Log.Error("API - AnswerCallbackQuery", ex.InnerMessageIfAny()); }
		}
	}
}