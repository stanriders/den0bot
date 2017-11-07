// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;
using den0bot.Osu;
using Meebey.SmartIrc4net;

namespace den0bot.Modules
{
    class ModAutohost : IModule
    {
        private IRC irc = new IRC();

#if !DEBUG
        private DateTime nextCheck;
        private readonly double check_interval = 30; //seconds
#endif

        private string currentHost;
        private int currentLobby = 0;
        private string CurrentChannel
        {
            get{ return $"#mp_{currentLobby}"; }
        }

        private List<string> UserList
        {
            get
            {
                List<string> userlist = irc.UserList(CurrentChannel);
                userlist.Remove("BanchoBot");
                userlist.Remove(Config.osu_irc_username);
                return userlist;
            }
        }

        public ModAutohost()
        {
            AddCommands(new Command[]
            {
                new Command()
                {
                    Name = "mplink",
                    Action = (msg) => { return $"{Config.osu_lobby_name} - {Config.osu_lobby_password} {Environment.NewLine}https://osu.ppy.sh/community/matches/{currentLobby}"; }
                },
                new Command()
                {
                    Name = "ircsend",
                    IsAdminOnly = true,
                    Action = (msg) => { irc.SendMessage(msg.Text.Substring(8), CurrentChannel); return string.Empty; }
                },
                new Command()
                {
                    Name = "mpuserlist",
                    Action = (msg) => GetUserlist()
                }
            });

            irc.OnMessage += OnIRCMessage;
            irc.Connect();
#if !DEBUG
            currentLobby = Database.CurrentLobbyID;

            if (currentLobby == 0)
                irc.SendMessage($"!mp make {Config.osu_lobby_name}", "BanchoBot");
            else
                irc.Join(CurrentChannel);

            nextCheck = DateTime.Now.AddMinutes(1);
#endif
            Log.Info(this, "Enabled");
        }

        private void OnIRCMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;

            if (message.StartsWith("Created the tournament match "))
            {
                int lobbynum = int.Parse(message.Substring(51).Split(' ')[0]);
                currentLobby = lobbynum;
                Database.CurrentLobbyID = lobbynum;

                SetupLobby();
            }
            else if (message.StartsWith("No such channel"))
            {
                irc.SendMessage($"!mp make {Config.osu_lobby_name}", "BanchoBot");
            }
            else if (message == "The match has finished!")
            {
                RotateHost();
            }
            else if (message.Contains(" joined in slot "))
            {
                if (UserList.Count <= 1)
                {
                    string username = message.Remove(message.IndexOf(" joined in slot "));
                    irc.SendMessage($"!mp host {username}", CurrentChannel);
                    currentHost = username;
                }
            }
            Log.IRC(e.Data.Nick, message);
        }

        private void SetupLobby()
        {
            irc.SendMessage($"!mp password {Config.osu_lobby_password}", CurrentChannel);
            irc.SendMessage("!mp size 16", CurrentChannel);
            irc.SendMessage("!mp unlock", CurrentChannel);
            irc.SendMessage("!mp mods Freemod", CurrentChannel);
        }

        private void RotateHost()
        {
            List<string> userlist = UserList;
            if (userlist.Count <= 1)
                return;

            int nextHost = userlist.LastIndexOf(currentHost) + 1;
            if (nextHost >= userlist.Count)
                nextHost = 0;

            irc.SendMessage($"!mp host {userlist[nextHost]}", CurrentChannel);
            currentHost = userlist[nextHost];
        }

        private string GetUserlist()
        {
            List<string> userlist = UserList;
            if (userlist != null)
            {
                if (userlist.Count <= 0)
                    return "Никто сейчас не играет";

                string result = string.Empty;
                foreach (string s in userlist)
                {
                    if (currentHost == s)
                        result += "(Host)";

                    result += s + Environment.NewLine;
                }
                return result;
            }
            return string.Empty;
        }

        public override void Think()
        {
#if !DEBUG
            if (nextCheck < DateTime.Now)
            {
                irc.Rejoin(CurrentChannel);
                nextCheck = DateTime.Now.AddSeconds(check_interval);
            }
#endif
        }
    }
}
