// den0bot (c) StanR 2021 - MIT License
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
			LoadConfig();
		}

		protected void AddCommand(Command command)
		{
			commands.Add(command);
		}

		protected void AddCommands(ICollection<Command> coll)
		{
			commands.AddRange(coll);
		}

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

		public async Task<bool> RunCommands(Message message)
		{
			var command = GetCommand(message.Text);
			if (command != null)
				return await command.Run(message);

			return false;
		}

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
