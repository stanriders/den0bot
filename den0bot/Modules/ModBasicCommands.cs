// den0bot (c) StanR 2019 - MIT License

using den0bot.DB;
using Telegram.Bot.Types.Enums;
using den0bot.Util;

namespace den0bot.Modules
{
	class ModBasicCommands : IModule
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
					Action = (message) =>
					{
						var locale = message.Text.Substring(11);
						if (Localization.GetAvailableLocales().Contains(locale))
						{
							Database.SetChatLocale(message.Chat.Id, locale);
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
					Action = (message) =>
					{
						var text = message.Text.Substring(17);
						Database.SetChatIntroduction(message.Chat.Id, text);
						return "👌";
					}
				},
			});
			Log.Debug("Enabled");
		}
	}
}
