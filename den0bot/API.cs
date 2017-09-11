// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using den0bot.DB;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot
{
    static class API
    {
        private static readonly string token = Config.telegam_token;
        private static TelegramBotClient api = new TelegramBotClient(token);
        private static Bot parent;

        public static bool IsConnected
        {
            get { return api.IsReceiving; }
        }

        /// <summary>
        /// Connect and start receiving messages. Returns false if failed to connect.
        /// </summary>
        /// <param name="b">Bot that called Connect and will receive all messages</param>
        public static bool Connect(Bot b)
        {
            parent = b;
            Log.Info("API", "Connecting...");
            try
            {
                api.OnMessage += ReceiveMessage;
                api.OnReceiveGeneralError += delegate (object sender, ReceiveGeneralErrorEventArgs args) { Log.Error("API - OnReceiveGeneralError", args.Exception.InnerMessageIfAny()); };
                api.OnReceiveError += delegate (object sender, ReceiveErrorEventArgs args) { Log.Error("API - OnReceiveError", args.ApiRequestException.InnerMessageIfAny()); };

                api.TestApiAsync();

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

        private static void ReceiveMessage(object sender, MessageEventArgs messageEventArgs)
        {
            parent.ProcessMessage(messageEventArgs.Message);
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">Text to send</param>
        /// <param name="receiver">Chat to send message to</param>
        /// <param name="mode">ParseMode to use (None/Markdown/HTML)</param>
        public static void SendMessage(string message, Chat receiver, ParseMode mode = ParseMode.Default) => SendMessage(message, receiver.Id, mode);

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">Text to send</param>
        /// <param name="receiverID">Chat ID to send message to</param>
        /// <param name="mode">ParseMode to use (None/Markdown/HTML)</param>
        public static void SendMessage(string message, long receiverID, ParseMode mode = ParseMode.Default)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                    api?.SendTextMessageAsync(receiverID, message, mode, true);
            }
            catch (Exception ex) { Log.Error("API - SendMessage", ex.InnerMessageIfAny()); }
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
                if (!receiver.DisableAnnouncements)
                {
                    if (!string.IsNullOrEmpty(image))
                        SendPhoto(image, receiver.Id, msg);
                    else if (!string.IsNullOrEmpty(msg))
                        SendMessage(msg, receiver.Id, mode);
                }
            }
        }

        /// <summary>
        /// Send photo
        /// </summary>
        /// <param name="photo">Photo to send. Can be both internal telegram photo ID or a link</param>
        /// <param name="receiver">Chat to send photo to</param>
        /// <param name="message">Photo caption if any</param>
        public static void SendPhoto(string photo, Chat receiver, string message = "") => SendPhoto(photo, receiver.Id, message);

        /// <summary>
        /// Send photo
        /// </summary>
        /// <param name="photo">Photo to send. Can be both internal telegram photo ID or a link</param>
        /// <param name="receiverId">Chat ID to send photo to</param>
        /// <param name="message">Photo caption if any</param>
        public static void SendPhoto(string photo, long receiverId, string message = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(photo))
                {
                    FileToSend file = new FileToSend();
                    if (photo.StartsWith("http") && (photo.EndsWith(".jpg") || photo.EndsWith(".png")))
                    {
                        file = UriPhotoDownload(new Uri(photo));
                        if (file.Content == null)
                        {
                            SendMessage(message, receiverId);
                            return;
                        }
                    }
                    else
                        file.FileId = photo;

                    api?.SendPhotoAsync(receiverId, file, message);
                }
            }
            catch (Exception ex) { Log.Error("API - SendPhoto", ex.InnerMessageIfAny()); }
        }
        private static FileToSend UriPhotoDownload(Uri link)
        {
            try
            {
                return new WebClient().OpenRead(link).ToFileToSend("yo");
            }
            catch (Exception ex)
            {
                Log.Error("API - UriPhotoDownload", ex.InnerMessageIfAny());
                return new FileToSend();
            }
        }

        /// <summary>
        /// Send sticker
        /// </summary>
        /// <param name="sticker">Telegram sticker ID</param>
        /// <param name="receiverID">Chat to send sticker to</param>
        public static void SendSticker(FileToSend sticker, long receiverID)
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
    }
}
