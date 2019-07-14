// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly string module_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Modules";

		private static bool IsOwner(string username) => (username == Config.Params.OwnerUsername);
		private static async Task<bool> IsAdmin(long chatID, string username)
		{
			if (IsOwner(username))
				return true;

			var admins = await API.GetAdmins(chatID);
			return admins.FirstOrDefault(x => x.User.Username == username) != null;
		}

		private static bool shouldShutdown;
		public static void Shutdown() { shouldShutdown = true; }

		public static void Main() => new Bot();

		private Bot()
		{
			Log.Info("________________");
			Config.Init();
			Database.Init();
			Localization.Init();

			Log.Info("Starting modules...");

			List<Assembly> allAssemblies = new List<Assembly>();
			if (Directory.Exists(module_path))
				foreach (string dll in Directory.GetFiles(module_path, "*.dll"))
					allAssemblies.Add(Assembly.LoadFile(dll));

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
						Log.Error($"{moduleName} not found!");
				}
			}
			else
			{
				Log.Error("Module list not found!");
			}
			Log.Info("Done!");

			//Osu.IRC.Connect();

			API.OnMessage += ProcessMessage;
			API.OnCallback += ProcessCallback;

			if (API.Connect())
			{
				Log.Info("Started thinking...");
				Think();
			}
			else
			{
				Log.Error("Can't connect to Telegram API!");
			}
			Log.Info("Exiting...");
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

			var senderChatId = msg.Chat.Id;

			if (msg.Chat.Type != ChatType.Private)
				Database.AddChat(senderChatId);

			Database.AddUser(msg.From.Id, msg.From.Username);

			if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
			{
				string greeting;
				if (msg.NewChatMembers[0].Id == API.BotUser.Id)
					greeting = Localization.Get("generic_added_to_chat", senderChatId);
				else
					greeting = Localization.NewMemberGreeting(senderChatId, msg.NewChatMembers[0].FirstName, msg.NewChatMembers[0].Id);

				await API.SendMessage(greeting, senderChatId, ParseMode.Html);
				return;
			}
			if (msg.Type != MessageType.Text &&
				msg.Type != MessageType.Photo)
				return;

			if (Config.Params.UseEvents && msg.Text != null && msg.Text.StartsWith("/"))
			{
				// random events
				string e = Events.Event(senderChatId);
				if (e != string.Empty)
				{
					await API.SendMessage(e, senderChatId);
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
						await messages.ReceiveMessage(msg);

					continue;
				}

				IModule.Command c = m.GetCommand(msg.Text);
				if (c != null)
				{
					if ((c.IsOwnerOnly && !IsOwner(msg.From.Username)) ||
					    (c.IsAdminOnly && !(await IsAdmin(msg.Chat.Id, msg.From.Username))))
					{
						// ignore admin commands from non-admins
						result = Localization.Get($"annoy_{RNG.NextNoMemory(1, 10)}", senderChatId);
						break;
					}

					// fire command's action
					if (c.Action != null)
						result = c.Action(msg);
					else if (c.ActionAsync != null)
						result = await c.ActionAsync(msg);
					else
						continue;

					// send result if we got any
					if (!string.IsNullOrEmpty(result))
					{
						parseMode = c.ParseMode;
						if (c.Reply)
							replyID = msg.MessageId;

						if (c.ActionResult != null)
						{
							var sentMessage = await API.SendMessage(result, senderChatId, parseMode, replyID);
							if (sentMessage != null)
							{
								// call action that receives sent message
								c.ActionResult(sentMessage);
								return;
							}
						}
						break;
					}
				}
			}

			await API.SendMessage(result, senderChatId, parseMode, replyID);
		}

		private async void ProcessCallback(object sender, CallbackQueryEventArgs callbackEventArgs)
		{
			foreach (IModule m in modules)
			{
				if (m is IReceiveCallback module)
				{
					//if (callbackEventArgs.CallbackQuery.Data == m.ToString())
					{
						var result = await module.ReceiveCallback(callbackEventArgs.CallbackQuery);
						await API.AnswerCallbackQuery(callbackEventArgs.CallbackQuery.Id, result);
					}
				}
			}
		}
	}
}
