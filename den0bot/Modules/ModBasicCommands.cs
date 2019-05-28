// den0bot (c) StanR 2019 - MIT License
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
					Name = "me",
					ParseMode = ParseMode.Markdown,
					Action = (msg) =>
					{
						API.RemoveMessage(msg.Chat.Id, msg.MessageId);
						return $"_{msg.From.FirstName}{msg.Text.Substring(3)}_";
					}
				},
				new Command
				{
					Names = {"start", "help"},
					Action = (msg) => msg.Chat.Type == ChatType.Private
						? Localization.Get("basiccommands_help", msg.Chat.Id)
						: string.Empty
				}
			});
			Log.Debug("Enabled");
		}
	}
}
