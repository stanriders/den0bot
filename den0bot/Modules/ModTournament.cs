// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using den0bot.Osu;
using Meebey.SmartIrc4net;

namespace den0bot.Modules
{
    class ModTournament : IModule
    {
        //private bool isMatchRunning = false;
        private Lobby lobby;
        private ModTournament()
        {
            AddCommands(new Command[]
            {
                new Command()
                {
                    Name = "startmatch",
                    IsAdminOnly = true,
                    Action = (msg) => StartMatch(msg.Text)
                },
                new Command()
                {
                    Name = "starttourney",
                    IsAdminOnly = true,
                    Action = (msg) => StartTourney(msg.Text)
                }
            });

            IRC.OnMessage += OnIRCMessage;
        }

        private void OnIRCMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;
        }

        private string StartMatch(string agrs)
        {
            lobby = new Lobby()
            {

            };
            //isMatchRunning = true;
            return string.Empty;
        }

        private string StartTourney(string agrs)
        {
            
            return string.Empty;
        }
    }
}
