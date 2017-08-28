using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Modules;
using den0bot.DB;

namespace den0bot
{
    public class Bot
    {
        private List<IModule> modules;
        private bool IsAdmin(long chatID, string username) => (username == "StanRiders") || (API.GetAdmins(chatID).Result.Find(x => x.User.Username == username) != null );

        public Bot()
        {
            Database.Init();

            modules = new List<IModule>()
            {
                new ModThread(),
                new ModYoutube(),
                new ModRandom(),
                new ModTopscores(),
                new ModProfile(),
                new ModBeatmap(),
                new ModMaplist(),
                new ModCat(),
                new ModSettings(),
                new ModPirate()
            };

            if (API.Connect(this))
            {
                Log.Info(this, "Started thinking...");
                Think();
            }
        }

        private void Think()
        {
            while (API.IsConnected)
            {
                foreach (IModule m in modules)
                {
                    m.Think();
                }
                Thread.Sleep(100);
            }
        }

        private string GreetNewfag(string username, long userID)
        {
            string result = $"Дороу, <a href=\"tg://user?id={userID}\">{username}</a>\n" +
                            "Хорошим тоном является:\n" +
                            "<b>1.</b> Кинуть профиль.\n" +
                            "<b>2.</b> Не инактивить.\n" +
                            "<b>3.</b> Словить бан при входе.\n" +
                            "<b>4.</b> Панду бить только ногами, иначе зашкваришься.\n" +
                            "Ден - аниме, но аниме запрещено. В мульти не играть - мужиков не уважать.\n" +
                            "<i>inb4 - бан</i>";

            return result;
        }

        public void ProcessMessage(Message msg)
        {
            Chat senderChat = msg.Chat;

            // having title means its a chat and not PM
            if (senderChat.Title != null)
                Database.AddChat(senderChat.Id);

            if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
            {
                API.SendMessage(GreetNewfag(msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id), senderChat, ParseMode.Html);
                return;
            }

            if (msg == null ||
                (msg.Type != MessageType.TextMessage &&
                msg.Type != MessageType.PhotoMessage) ||
                msg.ForwardFrom != null || 
                msg.ForwardFromChat != null || 
                msg.Date < DateTime.Now.ToUniversalTime().AddSeconds(-15))
                return;

            if (msg.Text != null && msg.Text.StartsWith("/"))
            {
                string e = Events.Event();
                if (e != string.Empty)
                {
                    API.SendMessage(e, senderChat);
                    return;
                }
            }

            ParseMode parseMode = ParseMode.Default;
            API.SendMessage(ProcessMessageWithModules(msg, ref parseMode), senderChat, parseMode);
        }

        private string ProcessMessageWithModules(Message msg, ref ParseMode parseMode)
        {
            foreach (IModule m in modules)
            {
                if (msg.Type == MessageType.PhotoMessage)
                {
                    if (!m.NeedsPhotos)
                        continue;
                    else
                        msg.Text = msg.Caption + " photo" + msg.Photo[0].FileId; //kinda hack
                }

                if (msg.Text.StartsWith("/") && !m.NeedsAllMessages)
                    msg.Text = msg.Text.Remove(0, 1);

                string result = string.Empty;
                if (m is IAdminOnly && IsAdmin(msg.Chat.Id, msg.From.Username) && msg.Chat.Title != null)
                {
                    IAdminOnly mAdmin = m as IAdminOnly;
                    result += mAdmin.ProcessAdminCommand(msg.Text, msg.Chat);
                }
                else
                {
                    result += m.ProcessCommand(msg.Text, msg.Chat);
                }

                if (result != string.Empty)
                {
                    parseMode = m.ParseMode;
                    return result;
                }
            }
            return string.Empty;
        }
    }
}
