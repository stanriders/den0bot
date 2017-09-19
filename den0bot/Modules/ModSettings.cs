// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;

namespace den0bot.Modules
{
    class ModSettings : IModule, IAdminOnly
    {
        public ModSettings() { Log.Info(this, "Enabled"); }
        public override void Think() { }
        public override string ProcessCommand(string msg, Telegram.Bot.Types.Chat sender) => string.Empty;
        public override bool NeedsPhotos => true;

        public string ProcessAdminCommand(string msg, Telegram.Bot.Types.Chat sender)
        {
            if (msg.StartsWith("disableannouncements"))
            {
                Database.ToggleAnnouncements(sender.Id, false);
                return "Понял, вырубаю";
            }
            else if (msg.StartsWith("enableannouncements"))
            {
                Database.ToggleAnnouncements(sender.Id, true);
                return "Понял, врубаю";
            }
            else if (msg.StartsWith("addmeme"))
            {
                string link = msg.Substring(8);

                if (link.StartsWith("http") && (link.EndsWith(".jpg") || link.EndsWith(".png")))
                {
                    Database.AddMeme(link, sender.Id);
                    return "Мемес добавлен!";
                }
                else if (link.StartsWith("photo"))
                {
                    Database.AddMeme(link.Substring(5), sender.Id);
                    return "Мемес добавлен!";
                }
                return "Ты че деб?";
            }
            else if (msg.StartsWith("addplayer"))
            {
                string name = msg.Split(' ')[1];
                string id = msg.Split(' ')[2];

                if (name != null && name != string.Empty)
                {
                    uint osuID = 0;
                    if (id != null && id != string.Empty)
                    {
                        try { osuID = uint.Parse(id); } catch (Exception) { }
                    }

                    Database.AddPlayer(name, osuID, sender.Id);
                    return $"Игрок добавлен! Имя {name}, профиль {osuID}";
                }
                return "Ты че деб?";
            }
            else if (msg.StartsWith("removeplayer"))
            {
                string name = msg.Substring(13);

                if (name != null && name != string.Empty)
                {
                    Database.RemovePlayer(name, sender.Id);
                    return $"{name} удален.";
                }
                return "Ты че деб?";
            }
            else if (msg.StartsWith("playerlist"))
            {
                string result = string.Empty;
                List<DB.Types.Player> players = Database.GetAllPlayers(sender.Id);
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
