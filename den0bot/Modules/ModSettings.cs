// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;

namespace den0bot.Modules
{
    class ModSettings : IModule
    {
        public override bool NeedsPhotos => true;
        public ModSettings()
        {
            AddCommands(new Command[]
            {
                new Command()
                {
                    Name = "disableannouncements",
                    IsAdminOnly = true,
                    Action = (msg) => { Database.ToggleAnnouncements(msg.Chat.Id, false); return "Понял, вырубаю"; }
                },
                new Command()
                {
                    Name = "enableannouncements",
                    IsAdminOnly = true,
                    Action = (msg) => { Database.ToggleAnnouncements(msg.Chat.Id, true); return "Понял, врубаю"; }
                },
                new Command()
                {
                    Name = "addmeme",
                    IsAdminOnly = true,
                    Action = (msg) => AddMeme(msg)
                },
                new Command()
                {
                    Name = "addplayer",
                    IsAdminOnly = true,
                    Action = (msg) => AddPlayer(msg)
                },
                new Command()
                {
                    Name = "removeplayer",
                    IsAdminOnly = true,
                    Action = (msg) => RemovePlayer(msg)
                },
                new Command()
                {
                    Name = "playerlist",
                    IsAdminOnly = true,
                    Action = (msg) => GetPlayerList(msg)
                },
            });
            Log.Info(this, "Enabled");
        }

        private string AddMeme(Telegram.Bot.Types.Message message)
        {
            long chatId = message.Chat.Id;
            string link = message.Text.Substring(8);

            if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
            {
                Database.AddMeme(link, chatId);
                return "Мемес добавлен!";
            }
            else if (link.StartsWith("photo"))
            {
                Database.AddMeme(link.Substring(5), chatId);
                return "Мемес добавлен!";
            }
            return "Ты че деб?";
        }

        private string AddPlayer(Telegram.Bot.Types.Message message)
        {
            string[] msg = message.Text.Split(' ');
            string username = msg[1];
            string name = msg[2];
            string id = msg[3];

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(name))
            {
                uint osuID = 0;
                if (id != null && id != string.Empty)
                {
                    try { osuID = uint.Parse(id); } catch (Exception) { }
                }

                Database.AddPlayer(username.Substring(1), name, osuID, message.Chat.Id);
                return $"{username.Substring(1)} добавлен! Имя {name}, профиль {osuID}";
            }
            return "Ты че деб?";
        }

        private string RemovePlayer(Telegram.Bot.Types.Message message)
        {
            string name = message.Text.Substring(13);

            if (name != null && name != string.Empty)
            {
                Database.RemovePlayer(name, message.Chat.Id);
                return $"{name} удален.";
            }
            return "Ты че деб?";
        }

        private string GetPlayerList(Telegram.Bot.Types.Message message)
        {
            string result = string.Empty;
            List<DB.Types.Player> players = Database.GetAllPlayers(message.Chat.Id);
            foreach (DB.Types.Player player in players)
            {
                result += $"{player.FriendlyName} - /u/{player.OsuID} - {player.Topscores}{Environment.NewLine}";
            }
            return result;
        }
    }
}
