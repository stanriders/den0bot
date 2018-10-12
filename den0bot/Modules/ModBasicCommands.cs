// den0bot (c) StanR 2018 - MIT License
using Telegram.Bot.Types.Enums;
using den0bot.Util;

namespace den0bot.Modules
{
    class ModBasicCommands : IModule
    {
        public ModBasicCommands()
        {
            AddCommands(new Command[]
{
                new Command()
                {
                    Name = "me",
                    ParseMode = ParseMode.Markdown,
                    Action = (msg) =>
                    {
                        API.RemoveMessage(msg.Chat.Id, msg.MessageId);
                        return $"_{msg.From.FirstName}{msg.Text.Substring(3)}_";
                    }
                },
                new Command()
                {
                    Name = "start",
                    Action = (msg) => { if (msg.Chat.Type == ChatType.Private) return Localization.Get("basiccommands_help", msg.Chat.Id); else return string.Empty; }
                },
                new Command()
                {
                    Name = "help",
                    Action = (msg) => { if (msg.Chat.Type == ChatType.Private) return Localization.Get("basiccommands_help", msg.Chat.Id); else return string.Empty; }
                },

            });
            Log.Info(this, "Enabled");
        }
    }
}
