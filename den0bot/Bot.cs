using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Modules;

namespace den0bot
{
    public class Bot
    {
        private List<IModule> modules;
        public static List<Chat> ChatList = new List<Chat>();

        public Bot()
        {
            modules = new List<IModule>();
        }

        public void Start()
        {
            modules.Add(new ModThread());
            modules.Add(new ModYoutube());
            modules.Add(new ModRandom());
            modules.Add(new ModTopscores());
            modules.Add(new ModProfile());
            modules.Add(new ModBeatmap());

            if (API.Connect(this))
            {
                Log.Info(this, "Started thinking...");
                Think();
            }
        }

        private string GreetNewfag(string username)
        {
            string result = "Дороу, " + username + "\n" +
                "Хорошим тоном является:\n" +
                "1. Кинуть профиль.\n" + 
                "2. Не инактивить.\n" + 
                "3. Словить бан при входе.\n" + 
                "4. Панду бить только ногами, иначе зашкваришься.\n" +
                "Ден - аниме, но аниме запрещено. В мульти не играть - мужиков не уважать.\n" + 
                "inb4 - бан";

            return result;
        }
        public void ProcessMessage(Message msg)
        {
            Chat senderChat = msg.Chat;

            if (senderChat.Title != null)
            {
                if (ChatList.Count > 0)
                {
                    if (ChatList.FindIndex(chat => chat.Id == senderChat.Id) == -1)
                    {
                        ChatList.Add(senderChat);
                        Log.Info(this, "Added chat '" + msg.Chat.Title + "' to the chat list");
                    }
                }
                else
                {
                    ChatList.Add(senderChat);
                    Log.Info(this, "Added chat '" + msg.Chat.Title + "' to the chat list");
                }
            }

            if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
            {
                API.SendMessage(GreetNewfag(msg.NewChatMembers[0].FirstName), senderChat);
                return;
            }

            if (msg == null || msg.Type != MessageType.TextMessage || msg.ForwardFrom != null || msg.ForwardFromChat != null || msg.Date < DateTime.Now.ToUniversalTime().AddMinutes(-0.5))
                return;

            API.SendMessage(ProcessCommand(msg, senderChat), senderChat);
        }
        public string ProcessCommand(Message message, Chat sender)
        {
            string result = string.Empty;
            string msg = message.Text;

            if (msg.StartsWith("/"))
            {
#if diggerTupoi
                if (message.From.Username == "firedigger" || message.From.Username == "@firedigger")
                    return Events.Annoy();
#endif

                string e = Events.Event();
                if (e != string.Empty)
                    return e;
            }

            foreach (IModule m in modules)
            {
                if (msg.StartsWith("/"))
                {
                    result += m.ProcessCommand(msg.Remove(0, 1), sender);
                }
                else if (m.NeedsAllMessages())
                {
                    result += m.ProcessCommand(msg, sender);
                }
            }

            return result;
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
    }
}
