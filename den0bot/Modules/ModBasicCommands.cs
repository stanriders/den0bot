// den0bot (c) StanR 2020 - MIT License

using den0bot.DB;
using Telegram.Bot.Types.Enums;
using den0bot.Util;
using System.Linq;

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
					Action = (msg) => msg.Chat.Type == ChatType.Private
						? Localization.Get("basiccommands_help", msg.Chat.Id)
						: string.Empty
				},
				new Command
				{
					Name = "me",
					ParseMode = ParseMode.Markdown,
					ActionAsync = async (msg) =>
					{
						await API.RemoveMessage(msg.Chat.Id, msg.MessageId);
						return $"_{msg.From.FirstName}{msg.Text.Substring(3)}_";
					}
				},
				new Command()
				{
					Name = "shutdownnow",
					IsOwnerOnly = true,
					Action = (msg) =>
					{
						Bot.Shutdown();
						return "Выключаюсь...";
					}
				},
				new Command()
				{
					Name = "setlocale",
					IsAdminOnly = true,
					ActionAsync = async (message) =>
					{
						var locale = message.Text.Substring(11);
						if (Localization.GetAvailableLocales().Contains(locale))
						{
							using (var db = new Database())
							{
								var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
								chat.Locale = locale;
								await db.SaveChangesAsync();
							}
							return "👌";
						}
						else
						{
							return "😡";
						}
					}
				},
				new Command()
				{
					Name = "setintroduction",
					IsAdminOnly = true,
					ActionAsync = async (message) =>
					{
						using (var db = new Database())
						{
							var text = message.Text.Substring(17);
							var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
							chat.Introduction = text;
							await db.SaveChangesAsync();
						}
						return "👌";
					}
				},
				new Command()
				{
					Name = "toggleevents",
					IsOwnerOnly = true,
					ActionAsync = async (message) =>
					{
						using (var db = new Database())
						{
							var chat = db.Chats.First(x=> x.Id == message.Chat.Id);
							chat.DisableEvents = !chat.DisableEvents;
							await db.SaveChangesAsync();
						
							return chat.DisableEvents.ToString();
						}
					}
				},
			});
			Log.Debug("Enabled");
		}
	}
}
