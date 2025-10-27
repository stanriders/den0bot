// den0bot (c) StanR 2021 - MIT License

using den0bot.DB;
using Telegram.Bot.Types.Enums;
using den0bot.Util;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Types;
using den0bot.Types.Answers;
using Microsoft.Extensions.Logging;

namespace den0bot.Modules
{
	internal class ModBasicCommands : IModule
	{
		public ModBasicCommands(ILogger<IModule> logger) : base(logger)
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
							var chat = DatabaseCache.Chats.First(x=> x.Id == message.Chat.Id);
							chat.Locale = locale;

							await DatabaseCache.UpdateChat(chat);

							return new TextCommandAnswer("👌");
						}

						return new TextCommandAnswer("😡");
					}
				},
				new Command
				{
					Name = "setintroduction",
					IsAdminOnly = true,
					ActionAsync = async (message) =>
					{
						var text = message.Text.Substring(17);
						var chat = DatabaseCache.Chats.First(x=> x.Id == message.Chat.Id);
						chat.Introduction = text;

						await DatabaseCache.UpdateChat(chat);

						return new TextCommandAnswer("👌");
					}
				},
				new Command
				{
					Name = "toggleevents",
					IsOwnerOnly = true,
					ActionAsync = async (message) =>
					{
						var chat = DatabaseCache.Chats.First(x=> x.Id == message.Chat.Id);
						chat.DisableEvents = !chat.DisableEvents;

						await DatabaseCache.UpdateChat(chat);

						return new TextCommandAnswer(chat.DisableEvents.ToString());
					}
				},
				new Command
				{
					Name = "getchatinfo",
					IsOwnerOnly = true,
					Action = message => new TextCommandAnswer($"ID: {message.Chat.Id}, From: {message.From?.Id}")
				},
			});
		}
	}
}
