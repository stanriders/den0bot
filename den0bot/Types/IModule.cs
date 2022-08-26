// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types;

namespace den0bot.Types
{
	public abstract class IModule
	{
		private readonly string configFile;
		private Dictionary<string, string> config = new();

		private readonly List<Command> commands = new();

		protected IModule()
		{
			configFile = $"./Modules/{GetType().Name}.json";
		}

		public virtual bool Init()
		{
			LoadConfig();
			Log.Debug("Enabled {Module}", GetType());
			return true;
		}

		/// <summary>
		/// Adds a single command to the command list
		/// </summary>
		/// <param name="command">Command</param>
		protected void AddCommand(Command command)
		{
			commands.Add(command);
		}

		/// <summary>
		/// Adds multiple commands to the command list
		/// </summary>
		/// <param name="coll">Commands</param>
		protected void AddCommands(ICollection<Command> coll)
		{
			commands.AddRange(coll);
		}

		/// <summary>
		/// Finds a command by the name in the text
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected Command GetCommand(string name)
		{
			if (string.IsNullOrEmpty(name) || commands.Count <= 0 || !name.StartsWith(Bot.command_trigger))
				return null;

			int nameEndIndex = name.IndexOf(' ');
			if (nameEndIndex != -1)
				name = name.Remove(nameEndIndex, name.Length - nameEndIndex);

			if (name.EndsWith($"@{API.BotUser.Username}"))
				name = name.Replace($"@{API.BotUser.Username}", "");

			return commands.Find(x => x.ContainsName(name.ToLowerInvariant()));
		}

		/// <summary>
		/// Runs message through command search and then executes the command if found
		/// </summary>
		/// <param name="message">Telegram message</param>
		/// <returns>Returns true if any of the commands were executed</returns>
		public async Task<bool> RunCommands(Message message)
		{
			var command = GetCommand(message.Text ?? message.Caption);
			if (command != null)
				return await command.Run(message);

			return false;
		}

		/// <summary>
		/// Function that runs every 100 ms
		/// </summary>
		public virtual void Think() { }

		private void LoadConfig()
		{
			if (System.IO.File.Exists(configFile))
				config = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(configFile));
		}

		protected string GetConfigVariable(string param)
		{
			if (!config.ContainsKey(param))
			{
				config.Add(param, string.Empty);
				System.IO.File.WriteAllText(configFile, JsonConvert.SerializeObject(config));
			}

			return config[param];
		}
	}
}
