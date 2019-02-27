// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
		private readonly List<IModule> modules = new List<IModule>();
		private const string module_path = "./Modules";

		private static bool IsOwner(string username) => (username == Config.Params.OwnerUsername);
		private static bool IsAdmin(long chatID, string username) => IsOwner(username) || (API.GetAdmins(chatID).Exists(x => x.User.Username == username));

		private static bool shouldShutdown;
		public static void Shutdown() { shouldShutdown = true; }

		public static void Main() => new Bot();

		private Bot()
		{
			Log.Info(this, "________________");
			Config.Init();
			Database.Init();
			Localization.Init();

			Log.Info(this, "Starting modules...");

			List<Assembly> allAssemblies = new List<Assembly>();
			foreach (string dll in Directory.GetFiles(module_path, "*.dll"))
				allAssemblies.Add(Assembly.Load(dll));

			if (Config.Params.Modules != null)
			{
				foreach (var moduleName in Config.Params.Modules)
				{
					// if it's local
					Type type = Type.GetType($"den0bot.Modules.{moduleName}", false);
					if (type == null)
					{
						// if its not local
						foreach (var ass in allAssemblies)
						{
							// we only allow subclases of IModule and only if they're in the config
							type = ass.GetTypes().FirstOrDefault(t => t.IsPublic && t.IsSubclassOf(typeof(IModule)) && t.Name == moduleName);

							if (type != null)
								break;
						}
					}
					if (type != null)
						modules.Add((IModule) Activator.CreateInstance(type));
					else
						Log.Error(this, $"{moduleName} not found!");
				}
			}
			else
			{
				Log.Error(this, "Module list not found!");
			}
			Log.Info(this, "Done!");

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
			Log.Info(this, "Exiting...");
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

		private async void ProcessMessage(object sender, MessageEventArgs messageEventArgs)
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

			Database.AddUser(msg.From.Id, msg.From.Username);

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

				// not a command
				if (msg.Text[0] != '/')
				{
					// send all messages to modules that need them
					if (m is IReceiveAllMessages messages)
						messages.ReceiveMessage(msg);

					continue;
				}

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

		private void ProcessCallback(object sender, CallbackQueryEventArgs callbackEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveCallback module)
				{
					//if (callbackEventArgs.CallbackQuery.Data == m.ToString())
					{
						module.ReceiveCallback(callbackEventArgs.CallbackQuery);
					}
				}
			}
		}
	}
}
