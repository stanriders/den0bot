// den0bot (c) StanR 2018 - MIT License
using System;
using Telegram.Bot.Types.Enums;
using den0bot.Util;

namespace den0bot.Modules
{
    class ModBasicCommands : IModule
    {
        private string helpText = "Дарова. Короче помимо того, что в списке команд я могу ещё:" + Environment.NewLine + Environment.NewLine +
                    "/addplayer - добавить игрока в базу. Синтаксис: /addplayer <юзернейм> <имя> <osu!айди>. Бот будет следить за новыми топскорами и сообщать их в чат. Также имя используется в базе щитпостеров." + Environment.NewLine +
                    "/removeplayer - убрать игрока из базы. Синтаксис: /removeplayer <имя, указанное при добавлении>." + Environment.NewLine +
                    "/addmeme - добавить мемес базу, можно как ссылку на картинку из интернета, так и загрузить её самому, а команду прописать в подпись." + Environment.NewLine +
                    "/disableannouncements - отключить оповещения о новых скорах кукизи." + Environment.NewLine +
                    "/enableannouncements - включить их обратно." + Environment.NewLine + Environment.NewLine +
                    "Все эти команды доступны только админам конфы. По вопросам насчет бота писать @StanRiders, но лучше не писать.";

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
                    Action = (msg) => { if (msg.Chat.Type == ChatType.Private) return helpText; else return string.Empty; }
                },
                new Command()
                {
                    Name = "help",
                    Action = (msg) => { if (msg.Chat.Type == ChatType.Private) return helpText; else return string.Empty; }
                },

            });
            Log.Info(this, "Enabled");
        }
    }
}
