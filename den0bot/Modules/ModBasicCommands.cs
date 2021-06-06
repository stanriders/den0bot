// den0bot (c) StanR 2021 - MIT License

using den0bot.DB;
using Telegram.Bot.Types.Enums;
using den0bot.Util;
using System.Linq;
using den0bot.Types;
using den0bot.Types.Answers;

namespace den0bot.Modules
{
	internal class ModBasicCommands : IModule
	{
		public ModBasicCommands()
		{
			AddCommands(new[]
			{
				new Command
				{
					Names = {"start", "help"},
					Action = (msg) => 
					{
						if (msg.Chat.Type == ChatType.Private)
							return Localization.GetAnswer("basiccommands_help", msg.Chat.Id);

						return null;
					}
				},
				new Command
				{
					Name = "me",
					ParseMode = ParseMode.Markdown,
					ActionAsync = async (msg) =>
					{
						await API.RemoveMessage(msg.Chat.Id, msg.MessageId);
						return new TextCommandAnswer($"_{msg.From.FirstName}{msg.Text.Substring(3)}_");
					}
				},
				new Command
				{
					Name = "shutdownnow",
					IsOwnerOnly = true,
					Action = (msg) =>
					{
						Bot.Shutdown();
						return new TextCommandAnswer("Выключаюсь...");
					}
				},
				new Command
				{
					Name = "restartnow",
					IsOwnerOnly = true,
					Action = (msg) =>
					{
						// a hack to safely restart bot without using service manager
						Bot.Shutdown(true);
						return new TextCommandAnswer("Перезагружаюсь...");
					}
				},
				new Command
				{
					Name = "setlocale",
					IsAdminOnly = true,
					ActionAsync = async (message) =>
					{
						var locale = message.Text.Substring(11);
						if (Localization.GetAvailableLocales().Contains(locale))
						{
							await using (var db = new Database())
							{
								var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
								chat.Locale = locale;
								await db.SaveChangesAsync();
							}
							return new TextCommandAnswer("👌");
						}
						else
						{
							return new TextCommandAnswer("😡");
						}
					}
				},
				new Command
				{
					Name = "setintroduction",
					IsAdminOnly = true,
					ActionAsync = async (message) =>
					{
						await using (var db = new Database())
						{
							var text = message.Text.Substring(17);
							var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
							chat.Introduction = text;
							await db.SaveChangesAsync();
						}
						return new TextCommandAnswer("👌");
					}
				},
				new Command
				{
					Name = "toggleevents",
					IsOwnerOnly = true,
					ActionAsync = async (message) =>
					{
						await using (var db = new Database())
						{
							var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
							chat.DisableEvents = !chat.DisableEvents;
							await db.SaveChangesAsync();
						
							return new TextCommandAnswer(chat.DisableEvents.ToString());
						}
					}
				},
			});
		}
	}
}
