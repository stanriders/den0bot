// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;

namespace den0bot.Modules
{
    class ModSettings : IModule, IHasAdminCommands
    {
        public ModSettings() { Log.Info(this, "Enabled"); }
        public override void Think() { }
        public override string ProcessCommand(Telegram.Bot.Types.Message message) => string.Empty;
        public override bool NeedsPhotos => true;

        public string ProcessAdminCommand(Telegram.Bot.Types.Message message)
        {
            string msg = message.Text;
            long chatId = message.Chat.Id;
            if (msg.StartsWith("disableannouncements"))
            {
                Database.ToggleAnnouncements(chatId, false);
                return "Понял, вырубаю";
            }
            else if (msg.StartsWith("enableannouncements"))
            {
                Database.ToggleAnnouncements(chatId, true);
                return "Понял, врубаю";
            }
            else if (msg.StartsWith("addmeme"))
            {
                string link = msg.Substring(8);

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
            else if (msg.StartsWith("addplayer"))
            {
                string username = msg.Split(' ')[1];
                string name = msg.Split(' ')[2];
                string id = msg.Split(' ')[3];

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(name))
                {
                    uint osuID = 0;
                    if (id != null && id != string.Empty)
                    {
                        try { osuID = uint.Parse(id); } catch (Exception) { }
                    }

                    Database.AddPlayer(username.Substring(1), name, osuID, chatId);
                    return $"{username.Substring(1)} добавлен! Имя {name}, профиль {osuID}";
                }
                return "Ты че деб?";
            }
            else if (msg.StartsWith("removeplayer"))
            {
                string name = msg.Substring(13);

                if (name != null && name != string.Empty)
                {
                    Database.RemovePlayer(name, chatId);
                    return $"{name} удален.";
                }
                return "Ты че деб?";
            }
            else if (msg.StartsWith("playerlist"))
            {
                string result = string.Empty;
                List<DB.Types.Player> players = Database.GetAllPlayers(chatId);
                foreach (DB.Types.Player player in players)
                {
                    result += $"{player.FriendlyName} - /u/{player.OsuID} - {player.Topscores}{Environment.NewLine}";
                }
                return result;
            }
            /*
            else if (msg.StartsWith("kick"))
            {
                string username = msg.Substring(12);

                if (username.StartsWith("@"))
                    username = username.Substring(1);

                //API.api.KickChatMemberAsync()
            }*/
            return string.Empty;
        }
    }
}
