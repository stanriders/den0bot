
using System;

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
                api.OnReceiveGeneralError += delegate (object sender, ReceiveGeneralErrorEventArgs args) { Log.Error("API", args.Exception.Message); };

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
            Message msg = messageEventArgs.Message;

            parent.ProcessMessage(msg);
        }

        public static void SendMessage(string message, Chat receiver, ParseMode mode = ParseMode.Default)
        {
            try
            {
                api?.SendTextMessageAsync(receiver.Id, message, mode, true);
            }
            catch (Exception ex) { Log.Error("API", ex.Message); }
        }

        public static void SendMessageToAllChats(string msg, string image = null, ParseMode mode = ParseMode.Default)
        {
            foreach (Chat receiver in Bot.ChatList)
            {
                if (image != null)
                    SendPhoto(image, receiver, msg);
                else
                    SendMessage(msg, receiver, mode);
            }
        }

        public static void SendPhoto(string photo, Chat receiver, string message = "")
        {
            try
            {
                api?.SendPhotoAsync(receiver.Id, new FileToSend(new Uri(photo)), message);
            }
            catch (Exception ex) { Log.Error("API", ex.Message); }
        }
    }
}
