// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using den0bot.Events;
using den0bot.Util;
using Sentry;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using CallbackQueryEventArgs = den0bot.Events.CallbackQueryEventArgs;
using MessageEventArgs = den0bot.Events.MessageEventArgs;
using User = Telegram.Bot.Types.User;

namespace den0bot
{
	public static class API
	{
		public static User BotUser { get; private set; }
		private static TelegramBotClient api;

		/// <summary>
		/// Connect and start receiving messages. Returns false if failed to connect.
		/// </summary>
		public static bool Connect()
		{
			if (string.IsNullOrEmpty(Config.Params.TelegamToken))
			{
				Log.Error("Telegram token is null or empty!");
				return false;
			}

			Log.Information("Connecting...");
			api = new TelegramBotClient(Config.Params.TelegamToken);
			if (!api.TestApiAsync().Result)
			{
				Log.Error("API Test failed, shutting down!");
				return false;
			}
			BotUser = api.GetMeAsync().Result;

			Log.Information("API Test successful, starting receiving...");
			api.StartReceiving(new DefaultUpdateHandler(HandleUpdates, HandleError));

			Log.Information("Connected!");
			return true;
		}

		public static event EventHandler<MessageEventArgs> OnMessage;
		public static event EventHandler<MessageEditEventArgs> OnMessageEdit;
		public static event EventHandler<CallbackQueryEventArgs> OnCallback;

		private static Task HandleUpdates(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			switch (update.Type)
			{
				case UpdateType.Message:
					OnMessage?.Invoke(api, new MessageEventArgs(update.Message));
					break;
				case UpdateType.EditedMessage:
					OnMessageEdit?.Invoke(api, new MessageEditEventArgs(update.EditedMessage));
					break;
				case UpdateType.CallbackQuery:
					OnCallback?.Invoke(api, new CallbackQueryEventArgs(update.CallbackQuery));
					break;
			}

			return Task.CompletedTask;
		}

		private static Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			Log.Error(exception.InnerMessageIfAny());

			return Task.CompletedTask;
		}

		/// <summary>
		/// Send message
		/// </summary>
		/// <param name="message">Text to send</param>
		/// <param name="chatId">Chat ID to send message to</param>
		/// <param name="parseMode">ParseMode to use (None/Markdown/HTML)</param>
		/// <param name="replyToId">Message ID to reply to</param>
		/// <param name="replyMarkup"></param>
		/// <param name="disablePreview"></param>
		public static async Task<Message> SendMessage(string message, long chatId, ParseMode? parseMode = null, int replyToId = 0, IReplyMarkup replyMarkup = null, bool disablePreview = true)
		{
			SentrySdk.ConfigureScope(scope =>
				{
					scope.Contexts["OutData"] = new
					{
						ChatID = chatId,
						Message = message,
						ParseMode = parseMode,
						ReplyID = replyToId,
						ReplyMarkup = replyMarkup,
						DisablePreview = disablePreview
					};
				}
			);

			try
			{
				if (!string.IsNullOrEmpty(message))
				{
					return await api.SendTextMessageAsync(chatId, message, parseMode,
						disableWebPagePreview: disablePreview,
						replyToMessageId: replyToId,
						replyMarkup: replyMarkup);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Send photo
		/// </summary>
		/// <param name="photo">Photo to send. Can be both internal telegram photo ID or a link</param>
		/// <param name="chatId">Chat ID to send photo to</param>
		/// <param name="caption">Photo caption if any</param>
		/// <param name="parseMode">ParseMode</param>
		/// <param name="replyToId">Message to reply to</param>
		/// <param name="replyMarkup"></param>
		/// <param name="sendTextIfFailed"></param>
		public static async Task<Message> SendPhoto(string photo, long chatId, string caption = "", ParseMode? parseMode = null, int replyToId = 0, IReplyMarkup replyMarkup = null, bool sendTextIfFailed = true)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					Photo = photo,
					ChatID = chatId,
					Message = caption,
					ParseMode = parseMode,
					ReplyID = replyToId,
					ReplyMarkup = replyMarkup,
					SendTextIfFailed = sendTextIfFailed
				};
			});

			try
			{
				if (!string.IsNullOrEmpty(photo))
				{
					return await api.SendPhotoAsync(chatId, new InputOnlineFile(photo), caption, parseMode, 
						replyToMessageId: replyToId, 
						replyMarkup: replyMarkup);
				}
			}
			catch (ApiRequestException ex)
			{
				Log.Error(ex.InnerMessageIfAny());
				if (sendTextIfFailed)
				{
					return await api.SendTextMessageAsync(chatId, caption, parseMode,
						replyToMessageId: replyToId,
						replyMarkup: replyMarkup);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Send multiple photos in an album
		/// </summary>
		/// <param name="photos">Array of InputMediaPhoto photos</param>
		/// <param name="chatId">Chat to send photos to</param>
		public static async Task<Message[]> SendMultiplePhotos(List<InputMediaPhoto> photos, long chatId)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					Photos = photos,
					ChatID = chatId
				};
			});

			try
			{
				if (photos != null && photos.Count > 1)
				{
					return await api.SendMediaGroupAsync(chatId, photos);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Send sticker
		/// </summary>
		/// <param name="sticker">Telegram sticker ID</param>
		/// <param name="chatId">Chat to send sticker to</param>
		public static async Task<Message> SendSticker(InputOnlineFile sticker, long chatId)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					Sticker = sticker,
					ChatID = chatId
				};
			});

			try
			{
				return await api.SendStickerAsync(chatId, sticker);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Returns array of chat members that are admins
		/// </summary>
		public static async Task<ChatMember[]> GetAdmins(long chatId)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["ChatID"] = chatId;
			});

			try
			{
				return await api.GetChatAdministratorsAsync(chatId);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Remove message from a chat if bot have enough rights
		/// </summary>
		/// <param name="chatId">Chat ID to remove message from</param>
		/// <param name="msgId">Message to remove</param>
		public static async Task RemoveMessage(long chatId, int msgId)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					ChatID = chatId,
					MessageID = msgId
				};
			});

			try
			{
				await api.DeleteMessageAsync(chatId, msgId);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
		}

		/// <summary>
		/// Edit message
		/// </summary>
		/// <param name="chatId">Chat ID to edit message in</param>
		/// <param name="messageId">Message to edit</param>
		/// <param name="message">New message</param>
		/// <param name="replyMarkup"></param>
		/// <param name="parseMode"></param>
		/// <param name="disablePreview"></param>
		public static async Task<Message> EditMessage(long chatId, int messageId, string message, InlineKeyboardMarkup replyMarkup = null, ParseMode? parseMode = null, bool disablePreview = true)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					MessageID = messageId,
					ChatID = chatId,
					Message = message,
					ParseMode = parseMode,
					ReplyMarkup = replyMarkup
				};
			});

			try
			{
				return await api.EditMessageTextAsync(chatId, messageId, message, parseMode, 
					disableWebPagePreview: disablePreview, 
					replyMarkup: replyMarkup);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Edit media caption
		/// </summary>
		/// <param name="chatId">Chat ID to edit message in</param>
		/// <param name="messageId">Message to edit</param>
		/// <param name="caption">New caption</param>
		/// <param name="replyMarkup"></param>
		/// <param name="parseMode"></param>
		public static async Task<Message> EditMediaCaption(long chatId, int messageId, string caption, InlineKeyboardMarkup replyMarkup = null, ParseMode? parseMode = null)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					MessageID = messageId,
					ChatID = chatId,
					Message = caption,
					ParseMode = parseMode,
					ReplyMarkup = replyMarkup
				};
			});

			try
			{
				return await api.EditMessageCaptionAsync(chatId, messageId, caption, 
					replyMarkup: replyMarkup, 
					parseMode: parseMode);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Answer callback query after receiving it
		/// </summary>
		/// <param name="callbackId">Callback ID that we need to answer</param>
		/// <param name="text">Text to send</param>
		/// <param name="showAlert">Alert user</param>
		public static async Task AnswerCallbackQuery(string callbackId, string text = null, bool showAlert = false)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					CallbackID = callbackId,
					Message = text,
					ShowAlert = showAlert
				};
			});

			try
			{
				await api.AnswerCallbackQueryAsync(callbackId, text, showAlert);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
		}

		/// <summary>
		/// Send voice message (or just audio file if it's not vorbis ogg)
		/// </summary>
		/// <param name="audio">Audio to send</param>
		/// <param name="chatId">Chat ID to send photo to</param>
		/// <param name="caption"></param>
		/// <param name="parseMode"></param>
		/// <param name="replyToId"></param>
		/// <param name="duration"></param>
		public static async Task<Message> SendVoice(InputOnlineFile audio, long chatId, string caption = null, ParseMode? parseMode = null, int replyToId = 0, int duration = 0)
		{
			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					Audio = audio,
					Duration = duration,
					ChatID = chatId,
					Message = caption,
					ParseMode = parseMode,
					ReplyID = replyToId
				};
			});

			try
			{
				return await api.SendVoiceAsync(chatId, audio, caption, parseMode, 
					duration: duration, 
					replyToMessageId: replyToId);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return null;
		}

		/// <summary>
		/// Updates admin rights on a user 
		/// </summary>
		public static async Task<bool> UpdateAdmin(long chatId, long userId,
			bool? isAnonymous = null,
			bool? canManageChat = null,
			bool? canChangeInfo = null,
			bool? canPostMessages = null,
			bool? canEditMessages = null,
			bool? canDeleteMessages = null,
			bool? canManageVoiceChats = null,
			bool? canInviteUsers = null,
			bool? canRestrictMembers = null,
			bool? canPinMessages = null,
			bool? canPromoteMembers = null)
		{
			try
			{
				await api.PromoteChatMemberAsync(chatId, userId, isAnonymous, canManageChat, canChangeInfo, canPostMessages, canEditMessages, 
					canDeleteMessages, canManageVoiceChats, canInviteUsers, canRestrictMembers, canPinMessages, canPromoteMembers);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// Updates user permissions
		/// </summary>
		public static async Task<bool> UpdatePermissions(long chatId, long userId, ChatPermissions permissions)
		{
			try
			{
				await api.RestrictChatMemberAsync(chatId, userId, permissions);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// Removes user from ban list if they're in and kicks if they're not
		/// </summary>
		public static async Task<bool> UnbanUser(long chatId, long userId)
		{
			try
			{
				await api.UnbanChatMemberAsync(chatId, userId);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// Downloads file from the server using File ID
		/// </summary>
		/// <param name="fileId">File ID to download</param>
		/// <param name="path">Where to download</param>
		public static async Task<bool> DownloadFile(string fileId, string path)
		{
			if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(path))
				return false;

			SentrySdk.ConfigureScope(scope =>
			{
				scope.Contexts["OutData"] = new
				{
					FileId = fileId,
					Path = path
				};
			});

			await using var stream = new MemoryStream();

			var file = await api.GetFileAsync(fileId);
			await api.DownloadFileAsync(file.FilePath, stream);

			await System.IO.File.WriteAllBytesAsync(path, stream.ToArray());

			return true;
		}
	}
}