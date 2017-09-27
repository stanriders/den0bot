// den0bot (c) StanR 2017 - MIT License
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Meebey.SmartIrc4net;

namespace den0bot.Osu
{
    class IRC
    {
        private IrcClient irc = new IrcClient()
        {
            AutoRejoin = true,
            AutoRelogin = true,
            ActiveChannelSyncing = true,
            AutoJoinOnInvite = true
        };
        public IRC()
        {
            irc.OnChannelAction += MessageHandler;
            irc.OnChannelNotice += MessageHandler;
            irc.OnChannelMessage += MessageHandler;
            irc.OnQueryAction += MessageHandler;
            irc.OnQueryNotice += MessageHandler;
            irc.OnQueryMessage += MessageHandler;
            irc.OnMotd += delegate (object sender, MotdEventArgs e) { Log.Info(this, e.Data.Message); }; ;
            irc.OnError += delegate (object sender, ErrorEventArgs e) { Log.Error(this, e.ErrorMessage); };
            irc.OnErrorMessage += MessageHandler;
        }
        public void Connect()
        {
            new Thread(new ThreadStart(delegate ()
            {
                irc.Connect("irc.ppy.sh", 6667);
                irc.Login(Config.osu_irc_username, "den0bot", 0, Config.osu_irc_username, Config.osu_irc_password);
                irc.Listen();
            })){ Name = "IRCThread" }.Start();
        }
        public void MessageHandler(object sender, IrcEventArgs e)
        {
            OnMessage(sender, e);
        }

        public event IrcEventHandler OnMessage;

        public void SendMessage(string msg, string channel)
        {
            irc.SendMessage(SendType.Message, channel, msg);
        }

        public void Join(string name)
        {
            irc.RfcJoin(name);
        }
        public void Rejoin(string name)
        {
            irc.RfcPart(name);
            irc.RfcJoin(name);
        }

        public List<string> UserList(string channel)
        {
            Channel chan = irc.GetChannel(channel);
            if (chan != null)
                return chan.Users.Keys.Cast<string>().ToList();

            return null;
        }
    }
}
