// den0bot (c) StanR 2018 - MIT License
using System.Collections.Generic;
using System.Linq;
using Meebey.SmartIrc4net;
using den0bot.Util;

namespace den0bot.Osu
{
	public class Lobby
	{
		public uint ID { get; private set; }
		public string Channel => $"#mp_{ID}";

		public string Name { get; set; }
		public string Password { get; set; }
		public int Size { get; set; } = 16;

		public string Link => $"https://osu.ppy.sh/community/matches/{ID}";

		public int CurrentHost { get; private set; }
		public Dictionary<int, string> UserList = new Dictionary<int, string>();

		private bool isCreated = false;

		public Lobby(uint id = 0)
		{
			IRC.OnMessage += OnIRCMessage;
			if (id == 0)
			{
				IRC.SendMessage($"!mp make {Name}", "BanchoBot");
			}
			else
			{
				ID = id;
				IRC.Join(Channel);
			}
		}

		public void RotateHost(int slot)
		{
			IRC.SendMessage($"!mp host {UserList[slot]}", Channel);
			CurrentHost = slot;
		}

		private void Setup()
		{
			IRC.SendMessage($"!mp password {Password}", Channel);
			IRC.SendMessage($"!mp size {Size}", Channel);
			IRC.SendMessage("!mp unlock", Channel);
			IRC.SendMessage("!mp mods Freemod", Channel);

			isCreated = true;
		}

		private void OnIRCMessage(object sender, IrcEventArgs e)
		{
			string message = e.Data.Message;
			if (message.StartsWith("Created the tournament match ") && !isCreated)
			{
				ID = uint.Parse(message.Substring(51).Split(' ')[0]);
				Setup();
			}
			else if (message.Contains(" joined in slot "))
			{
				string username = message.Remove(message.IndexOf(" joined in slot "));
				int slot = int.Parse(message.Substring(message.LastIndexOf(" joined in slot ")));
				if (UserList.Count <= 1)
				{
					RotateHost(slot);
				}
				UserList.Add(slot, username);
				UserList = UserList.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
			}
			else if (message.Contains(" left the game."))
			{
				string username = message.Remove(message.IndexOf(" left the game."));
				UserList.Remove(UserList.GetKeyByValue(username));
				UserList = UserList.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
			}
			else if (message.Contains(" moved to slot "))
			{
				string username = message.Remove(message.IndexOf(" moved to slot "));
				int slot = int.Parse(message.Substring(message.LastIndexOf(" moved to slot ")));
				UserList.Remove(UserList.GetKeyByValue(username));
				UserList.Add(slot, username);
				UserList = UserList.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
			}
		}
	}
}