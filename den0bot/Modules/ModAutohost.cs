// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using den0bot.DB;
using den0bot.Osu;
using den0bot.Util;
using Meebey.SmartIrc4net;

namespace den0bot.Modules
{
	class ModAutohost : IModule
	{
		private DateTime nextCheck;
		private readonly double check_interval = 30; //seconds

		private Lobby lobby;

		public ModAutohost()
		{
			AddCommands(new []
			{
				new Command
				{
					Name = "mplink",
					Action = msg => $"{Config.Params.osuLobbyName} - {Config.Params.osuLobbyPassword} {Environment.NewLine}{lobby.Link}"
				},
				new Command
				{
					Name = "mpuserlist",
					Action = (msg) => GetUserlist()
				}
			});

			IRC.OnMessage += OnIRCMessage;
			IRC.Connect();

			lobby = new Lobby((uint)Database.CurrentLobbyID)
			{
				Name = Config.Params.osuLobbyName,
				Password = Config.Params.osuLobbyPassword,
				Size = 16
			};

			nextCheck = DateTime.Now.AddMinutes(1);
			Log.Debug(this, "Enabled");
		}

		private void OnIRCMessage(object sender, IrcEventArgs e)
		{
			string message = e.Data.Message;

			if (message.StartsWith("No such channel"))
			{
				lobby = new Lobby()
				{
					Name = Config.Params.osuLobbyName,
					Password = Config.Params.osuLobbyPassword,
					Size = 16
				};
				Database.CurrentLobbyID = (int)lobby.ID;
			}
			else if (message == "The match has finished!")
			{
				RotateHost();
			}
			
			Log.IRC(e.Data.Nick, message);
		}

		private void RotateHost()
		{
			if (lobby.UserList.Count <= 1)
				return;

			int nextHost = lobby.CurrentHost + 1;
			if (nextHost >= lobby.UserList.Count)
				nextHost = 0;

			lobby.RotateHost(nextHost);
		}

		private string GetUserlist()
		{
			string result = string.Empty;
			foreach (KeyValuePair<int, string> user in lobby.UserList)
			{
				result += $"{user.Key}. {user.Value}{Environment.NewLine}";
			}

			if (string.IsNullOrEmpty(result))
				return "Сейчас никто не играет.";

			return result;
		}

		public override void Think()
		{
			if (nextCheck < DateTime.Now && lobby.UserList.Count <= 0)
			{
				IRC.Rejoin(lobby.Channel);
				nextCheck = DateTime.Now.AddSeconds(check_interval);
			}
		}
	}
}
