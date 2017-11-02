// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.Modules;
using den0bot.DB;
using Telegram.Bot.Args;

namespace den0bot
{
    public class Bot
    {
        private List<IModule> modules;
        private bool IsAdmin(long chatID, string username) => (username == "StanRiders") || (API.GetAdmins(chatID).Exists(x => x.User.Username == username) );
        private DateTime startupTime;

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
                new ModAutohost(),
                new ModRecentScores()
            };

            API.OnMessage += ProcessMessage;

            if (API.Connect())
            {
                Log.Info(this, "Started thinking...");
                startupTime = DateTime.Now;
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
            return $"Дороу, <a href=\"tg://user?id={userID}\">{username}</a>" + Environment.NewLine +
                    "Хорошим тоном является:" + Environment.NewLine +
                    "<b>1.</b> Кинуть профиль." + Environment.NewLine +
                    "<b>2.</b> Не инактивить." + Environment.NewLine +
                    "<b>3.</b> Словить бан при входе." + Environment.NewLine +
                    "<b>4.</b> Панду бить только ногами, иначе зашкваришься." + Environment.NewLine +
                    "Ден - аниме, но аниме запрещено. В мульти не играть - мужиков не уважать." + Environment.NewLine +
                    "<i>inb4 - бан</i>";
        }

        public async void ProcessMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Message msg = messageEventArgs.Message;

            if (msg == null ||
                msg.ForwardFrom != null ||
                msg.ForwardFromChat != null ||
                msg.Date < DateTime.Now.ToUniversalTime().AddSeconds(-15))
                return;

            Chat senderChat = msg.Chat;

            if (msg.Chat.Type != ChatType.Private)
                Database.AddChat(senderChat.Id);

            if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
            {
                API.SendMessage(GreetNewfag(msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id), senderChat, ParseMode.Html);
                return;
            }
            if (msg.Type != MessageType.TextMessage &&
                msg.Type != MessageType.PhotoMessage)
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

            string result = ProcessBasicCommands(msg, ref parseMode);
            if (result == string.Empty)
            {
                foreach (IModule m in modules)
                {
                    if (msg.Type == MessageType.PhotoMessage)
                    {
                        if (!m.NeedsPhotos)
                            continue;

                        msg.Text = msg.Caption + " photo" + msg.Photo[0].FileId; //kinda hack
                    }

                    if (msg.Text == null)
                        continue;

                    if (m is IProcessAllMessages)
                        (m as IProcessAllMessages).ReceiveMessage(msg);
                    else if (msg.Text[0] != '/')
                        continue;

                    Command c = m.GetCommand(msg.Text);
                    if (c != null)
                    {
                        if (c.IsAdminOnly && !IsAdmin(msg.Chat.Id, msg.From.Username) || msg.Chat.Type == ChatType.Private)
                            continue;

                        string res = string.Empty;
                        if (c.IsAsync)
                            res = await c.ActionAsync(msg);
                        else
                            res = c.Action(msg);

                        if (!string.IsNullOrEmpty(res))
                        {
                            parseMode = c.ParseMode;
                            result = res;
                            break;
                        }
                    }
                }
            }

            API.SendMessage(result, senderChat, parseMode);
        }

        private string ProcessBasicCommands(Message msg, ref ParseMode parseMode)
        {
            if (msg.Text == null || msg.Text == string.Empty || msg.Text[0] != '/')
                return string.Empty;

            string text = msg.Text;

            if (text.StartsWith("/me ")) //meh
            {
                API.RemoveMessage(msg.Chat.Id, msg.MessageId);
                parseMode = ParseMode.Markdown;
                return $"_{msg.From.FirstName}{text.Substring(3)}_";
            }
            else if ((msg.Chat.Type == ChatType.Private) && (text.StartsWith("/start") || text.StartsWith("/help")))
            {
                return "Дарова. Короче помимо того, что в списке команд я могу ещё:" + Environment.NewLine + Environment.NewLine +
                    "/addplayer - добавить игрока в базу. Синтаксис: /addplayer <имя> <osu!айди>. Бот будет следить за новыми топскорами и сообщать их в чат. Также имя используется в базе щитпостеров." + Environment.NewLine +
                    "/removeplayer - убрать игрока из базы. Синтаксис: /removeplayer <имя, указанное при добавлении>." + Environment.NewLine +
                    "/addmeme - добавить мемес базу, можно как ссылку на картинку из интернета, так и загрузить её самому, а команду прописать в подпись." + Environment.NewLine +
                    "/disableannouncements - отключить оповещения о новых скорах кукизи." + Environment.NewLine +
                    "/enableannouncements - включить их обратно." + Environment.NewLine + Environment.NewLine +
                    "Все эти команды доступны только админам конфы. По вопросам насчет бота писать @StanRiders, но лучше не писать." + Environment.NewLine +
                    "http://kikoe.ru/";
            }
            else if (text.StartsWith("/requestcount"))
            {
                if (IsAdmin(msg.Chat.Id, msg.From.Username))
                {
                    return (Osu.OsuAPI.RequestCount / ((DateTime.Now - startupTime).TotalSeconds / 60.0d)).ToString();
                }
            }

            return string.Empty;
        }
    }
}
