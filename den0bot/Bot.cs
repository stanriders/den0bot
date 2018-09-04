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
		private string GreetNewfag(string username, long userID) => $"Дороу, <a href=\"tg://user?id={userID}\">{username}</a>" + Environment.NewLine +
																	 "Хорошим тоном является:" + Environment.NewLine +
																	 "<b>1.</b> Кинуть профиль." + Environment.NewLine +
																	 "<b>2.</b> Не инактивить." + Environment.NewLine +
																	 "<b>3.</b> Словить бан при входе." + Environment.NewLine +
																	 "<b>4.</b> Панду бить только ногами, иначе зашкваришься." + Environment.NewLine +
																	 "Ден - аниме, но аниме запрещено. В мульти не играть - мужиков не уважать." + Environment.NewLine +
																	 "<i>inb4 - бан</i>";

		public static bool IsOwner(string username) => (username == Config.owner_username);
		public static bool IsAdmin(long chatID, string username) => IsOwner(username) || (API.GetAdmins(chatID).Exists(x => x.User.Username == username));

		private static bool shouldShutdown = false;
		public static void Shutdown() {shouldShutdown = true;}
		
		public Bot()
        {
            Database.Init();

            Osu.Oppai.CheckOppai();

            modules = new List<IModule>()
            {
                new ModThread(),
                new ModYoutube(),
                new ModRandom(),
                //new ModTopscores(),
                new ModProfile(),
                new ModBeatmap(),
                new ModMaplist(),
                new ModCat(),
                new ModSettings(),
                //new ModAutohost(),
                new ModRecentScores(),
                new ModGirls()
            };

            //Osu.IRC.Connect();

            API.OnMessage += ProcessMessage;

            if (API.Connect())
            {
                Log.Info(this, "Started thinking...");
                Think();
            }
        }

        private void Think()
        {
            while (API.IsConnected && !shouldShutdown)
            {
                foreach (IModule m in modules)
                {
                    m.Think();
                }
                Thread.Sleep(100);
            }
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
            if (msg.Type != MessageType.Text &&
                msg.Type != MessageType.Photo)
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
            int replyID = 0;

            string result = string.Empty;
            foreach (IModule m in modules)
            {
                if (msg.Type == MessageType.Photo)
                {
                    if (!m.NeedsPhotos)
                        continue;

                    msg.Text = msg.Caption + " photo" + msg.Photo[0].FileId; //kinda hack
                }

                // FIXME: move to trigger check?
                if (msg.Text == null)
                    continue;

                // send all messages to modules that need them
                if (m is IProcessAllMessages)
                    (m as IProcessAllMessages).ReceiveMessage(msg);

                // not a command
                if (msg.Text[0] != '/')
                    continue;

                IModule.Command c = m.GetCommand(msg.Text);
                if (c != null)
                {
                    if ((c.IsAdminOnly && !IsAdmin(msg.Chat.Id, msg.From.Username)) ||
						(c.IsOwnerOnly && !IsOwner(msg.From.Username)))
					{
                        // ignore admin commands from non-admins
                        result = Events.Annoy();
                        break;
                    }

                    string res = string.Empty;
                    if (c.Action != null)
                        res = c.Action(msg);
                    else if (c.ActionAsync != null)
                        res = await c.ActionAsync(msg);
                    
                    if (!string.IsNullOrEmpty(res))
                    {
                        parseMode = c.ParseMode;
                        if (c.Reply)
                            replyID = msg.MessageId;

                        result = res;

                        if (c.ActionResult != null)
                        {
                            var sentMessage = await API.SendMessage(result, senderChat.Id, parseMode, replyID);
                            if (sentMessage != null)
                            {
                                // call action that recieves sent message
                                c.ActionResult(sentMessage);
                                return;
                            }
                        }
                        break;
                    }
                }
            }

            API.SendMessage(result, senderChat, parseMode, replyID);
        }
    }
}
