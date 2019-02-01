// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	public abstract class IModule
	{
		public class Command
		{
			public List<string> Names { get; set; } = new List<string>();
			public string Name
			{
				set => Names.Add(value);
			}
			public bool ContainsName(string name)
			{
				return Names.Contains(name.Substring(1));
			}

			public ParseMode ParseMode;

			public bool IsAdminOnly;
			public bool IsOwnerOnly;
			public bool Reply;

			public Func<Message, Task<string>> ActionAsync;
			public Func<Message, string> Action;
			public Action<Message> ActionResult;
		}

		private readonly List<Command> commands = new List<Command>();

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
			if (commands.Count <= 0)
				return null;

			int nameEndIndex = name.IndexOf(' ');
			if (nameEndIndex != -1)
				name = name.Remove(nameEndIndex, name.Length - nameEndIndex);

			if (name.EndsWith("@den0bot"))
				name = name.Replace("@den0bot", "");

			return commands.Find(x => x.ContainsName(name.ToLowerInvariant()));
		}

		public virtual void Think() { }
	}
}
