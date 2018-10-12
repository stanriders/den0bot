// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using den0bot.DB;
using den0bot.Modules;
using den0bot.Util;

namespace den0bot
{
	public class Bot
	{
		private List<IModule> modules;

		public static bool IsOwner(string username) => (username == Config.owner_username);
		public static bool IsAdmin(long chatID, string username) => IsOwner(username) || (API.GetAdmins(chatID).Exists(x => x.User.Username == username));

		private static bool shouldShutdown = false;
		public static void Shutdown() {shouldShutdown = true;}
		
		public Bot()
        {
            Database.Init();
			Localization.Init();

			modules = new List<IModule>()
            {
				new ModBasicCommands(),
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
                new ModGirls(),
				new ModMatchFollow()
			};

            //Osu.IRC.Connect();

            API.OnMessage += ProcessMessage;
			API.OnCallback += ProcessCallback;

            if (API.Connect())
            {
                Log.Info(this, "Started thinking...");
                Think();
            }
			else
			{
				Log.Error(this, "Can't connect to Telegram API!");
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
			Database.Close();
			API.Disconnect();
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
				string greeting;
				if (msg.NewChatMembers[0].Id == API.BotUser.Id)
					greeting = Localization.Get("generic_added_to_chat", senderChat.Id);
				else
					greeting = Localization.NewMemberGreeting(senderChat.Id, msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id);

				API.SendMessage(greeting, senderChat, ParseMode.Html);
                return;
            }
            if (msg.Type != MessageType.Text &&
                msg.Type != MessageType.Photo)
                return;

            if (msg.Text != null && msg.Text.StartsWith("/"))
            {
                string e = Events.Event(senderChat.Id);
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
                    if (!(m is IReceivePhotos))
                        continue;

                    msg.Text = msg.Caption; // for consistency
                }

                // FIXME: move to trigger check?
                if (msg.Text == null)
                    continue;

                // send all messages to modules that need them
                if (m is IReceiveAllMessages)
                    (m as IReceiveAllMessages).ReceiveMessage(msg);

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
                        result = Events.Annoy(senderChat.Id);
                        break;
                    }

					// fire command's action
					string res = string.Empty;
                    if (c.Action != null)
                        res = c.Action(msg); 
                    else if (c.ActionAsync != null)
                        res = await c.ActionAsync(msg);
                    
					// send result if we got any
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

		public void ProcessCallback(object sender, CallbackQueryEventArgs callbackEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveCallback)
				{
					var module = m as IReceiveCallback;
					//if (callbackEventArgs.CallbackQuery.Data == m.ToString())
					{
						module.ReceiveCallback(callbackEventArgs.CallbackQuery);
					}
				}
			}
		}
	}
}
