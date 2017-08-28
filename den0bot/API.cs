
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static readonly string token = Config.telegam_token; //den0bot
        public static TelegramBotClient api = new TelegramBotClient(token);

        private static Bot parent;

        public static bool IsConnected
        {
            get { return api.IsReceiving; }
        }

        public static bool Connect(Bot b)
        {
            parent = b;
            Log.Info("API", "Connecting...");
            try
            {
                api.OnMessage += ReceiveMessage;
                api.OnReceiveGeneralError += delegate (object sender, ReceiveGeneralErrorEventArgs args) { Log.Error("API - OnReceiveGeneralError", args.Exception.InnerException?.Message); };

                api.TestApiAsync();

                api.StartReceiving();
            }
            catch (Exception ex)
            {
                Log.Error("API", ex.Message);
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

        public static void ReceiveMessage(object sender, MessageEventArgs messageEventArgs)
        {
            parent.ProcessMessage(messageEventArgs.Message);
        }

        public static void SendMessage(string message, Chat receiver, ParseMode mode = ParseMode.Default) => SendMessage(message, receiver.Id, mode);
        public static void SendMessage(string message, long receiverID, ParseMode mode = ParseMode.Default)
        {
            try
            {
                if (message != null && message != string.Empty)
                    api?.SendTextMessageAsync(receiverID, message, mode, true);
            }
            catch (Exception ex) { Log.Error("API - SendMessage", ex.Message); }
        }

        public static void SendMessageToAllChats(string msg, string image = null, ParseMode mode = ParseMode.Default)
        {
            foreach (DB.Types.Chat receiver in Database.GetAllChats())
            {
                if (!receiver.DisableAnnouncements)
                {
                    if (image != null && image != string.Empty)
                        SendPhoto(image, receiver.Id, msg);
                    else if (msg != null && msg != string.Empty)
                        SendMessage(msg, receiver.Id, mode);
                }
            }
        }

        public static void SendPhoto(string photo, Chat receiver, string message = "") => SendPhoto(photo, receiver.Id, message);
        public static void SendPhoto(string photo, long receiverId, string message = "")
        {
            try
            {
                if (photo != null && photo != string.Empty)
                {
                    FileToSend file = new FileToSend();
                    if (photo.StartsWith("http") && (photo.EndsWith(".jpg") || photo.EndsWith(".png")))
                        file.Url = new Uri(photo);
                    else
                        file.FileId = photo;

                    api?.SendPhotoAsync(receiverId, file, message);
                }
            }
            catch (Exception ex) { Log.Error("API - SendPhoto", ex.Message); }
        }
        
        
        public static async Task<List<ChatMember>> GetAdmins(long chatID)
        {
            ChatMember[] admins = await api?.GetChatAdministratorsAsync(chatID);
            return admins.ToList();
        }
    }
}
