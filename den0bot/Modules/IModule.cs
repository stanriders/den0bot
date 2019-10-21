// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	public abstract class IModule
	{
		public class Command
		{
			public List<string> Names { get; } = new List<string>();
			public string Name
			{
				set => Names.Add(value);
			}
			public bool ContainsName(string name)
			{
				return Names.Contains(name.Substring(1));
			}

			/// <summary>
			/// <see cref="Telegram.Bot.Types.Enums.ParseMode" />
			/// </summary>
			public ParseMode ParseMode { get; set; }

			/// <summary>
			/// Determines if command can only be used by chat admins
			/// </summary>
			public bool IsAdminOnly { get; set; }

			/// <summary>
			/// Determines if command can only be used by bot owner
			/// </summary>
			public bool IsOwnerOnly { get; set; }

			/// <summary>
			/// Determines if command result should be a reply to the command message
			/// </summary>
			public bool Reply { get; set; }

			public Func<Message, Task<string>> ActionAsync { get; set; }
			public Func<Message, string> Action { get; set; }

			/// <summary>
			/// Function to call AFTER action is complete and sent
			/// </summary>
			public Action<Message> ActionResult { get; set; }
		}

		private readonly string configFile;
		private Dictionary<string, string> config = new Dictionary<string, string>();

		private readonly List<Command> commands = new List<Command>();

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

		public Command GetCommand(string name)
		{
			if (string.IsNullOrEmpty(name) || commands.Count <= 0 || !name.StartsWith(Bot.command_trigger))
				return null;

			int nameEndIndex = name.IndexOf(' ');
			if (nameEndIndex != -1)
				name = name.Remove(nameEndIndex, name.Length - nameEndIndex);

			if (name.EndsWith("@den0bot"))
				name = name.Replace("@den0bot", "");

			return commands.Find(x => x.ContainsName(name.ToLowerInvariant()));
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
