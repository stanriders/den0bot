// den0bot (c) StanR 2020 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Util;
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

			private async Task<bool> IsAllowedAsync(Message message)
			{
				var from = message.From.Username;

				// owner can run any commands
				if (from == Config.Params.OwnerUsername)
					return true;

				if (IsOwnerOnly)
					return from == Config.Params.OwnerUsername;

				if (IsAdminOnly)
					return (await API.GetAdmins(message.Chat.Id)).Any(x => x.User.Username == from && (x.CanPromoteMembers ?? true));

				return true;
			}

			public async Task<bool> Run(Message message)
			{
				var senderChatId = message.Chat.Id;

				if (!await IsAllowedAsync(message))
				{
					// ignore admin commands from non-admins
					await API.SendMessage(Localization.Get($"annoy_{RNG.NextNoMemory(1, 10)}", senderChatId), senderChatId);
					return true;
				}

				var result = string.Empty;

				// fire command's action
				if (ActionAsync != null)
					result = await ActionAsync(message);
				else if(Action != null)
					result = Action(message);

				// send result if we got any
				if (!string.IsNullOrEmpty(result))
				{
					if (ActionResult != null)
					{
						var sentMessage = await API.SendMessage(result, senderChatId, ParseMode, Reply ? message.MessageId : 0);
						if (sentMessage != null)
						{
							// call action that receives sent message
							ActionResult(sentMessage);
							return true;
						}
					}

					await API.SendMessage(result, senderChatId, ParseMode, Reply ? message.MessageId : 0);
					return true;
				}
				return false;
			}
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
