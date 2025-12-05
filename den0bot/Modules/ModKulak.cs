// den0bot (c) StanR 2025 - MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace den0bot.Modules
{
	public class KulakState
	{
		public int MessageId { get; set; }
		public long[] PressedBy { get; set; }
		public DateTime StartedAt { get; set; }
		public string MessageText { get; set; }
	}

	public class ModKulak : IModule, IReceiveCallbacks
	{
		private readonly Dictionary<long, KulakState> _kulakState = new();
		private readonly Dictionary<long, KulakState> _sorryState = new();

		private readonly InlineKeyboardMarkup _kulakButton = new(
			new[] { new InlineKeyboardButton("🤛") { CallbackData = "kulak" },
			}
		);

		private readonly InlineKeyboardMarkup _sorryButton = new(
			new[] { new InlineKeyboardButton("✋") { CallbackData = "sorry" },
			}
		);

		private const int seconds = 60 * 3;

		public ModKulak(ILogger<IModule> logger) : base(logger)
		{
			AddCommands(new[]
			{
				new Command
				{
					Names = { "kulak", "fist" },
					ActionAsync = Kulak
				},
				new Command
				{
					Name = "sorry",
					ActionAsync = Sorry
				},
			});
		}

		private async Task<ICommandAnswer> Sorry(Message message)
		{
			if (message?.From == null)
				return null;

			if (_sorryState.TryGetValue(message.Chat.Id, out var existingSorry))
			{
				if (existingSorry.StartedAt < DateTime.Now.AddSeconds(-seconds))
				{
					_sorryState.Remove(message.Chat.Id);
					await SendNewSorry(message);
				}
				else if (existingSorry.PressedBy.Contains(message.From.Id))
				{
					await API.RemoveMessage(message.Chat.Id, message.Id);
				}
				else
				{
					existingSorry.PressedBy = [.. existingSorry.PressedBy, message.From.Id];
					existingSorry.MessageText += $"\n✋{message.From!.FirstName} {message.From!.LastName}";
					await API.EditMessage(message.Chat.Id, existingSorry.MessageId, existingSorry.MessageText, _sorryButton);
				}
			}
			else
			{
				await SendNewSorry(message);
			}

			return null;
		}

		private async Task<ICommandAnswer> Kulak(Message message)
		{
			if (message?.From == null)
				return null;

			if (_kulakState.TryGetValue(message.Chat.Id, out var existingKulak))
			{
				if (existingKulak.StartedAt < DateTime.Now.AddSeconds(-seconds))
				{
					_kulakState.Remove(message.Chat.Id);
					await SendNewKulak(message);
				}
				else if (existingKulak.PressedBy.Contains(message.From.Id))
				{
					await API.RemoveMessage(message.Chat.Id, message.Id);
				}
				else
				{
					existingKulak.PressedBy = [.. existingKulak.PressedBy, message.From.Id];
					existingKulak.MessageText += $"\n🤛{message.From!.FirstName} {message.From!.LastName}";
					await API.EditMessage(message.Chat.Id, existingKulak.MessageId, existingKulak.MessageText, _kulakButton);
				}
			}
			else
			{
				await SendNewKulak(message);
			}

			return null;
		}

		private async Task SendNewKulak(Message message)
		{
			var messageText = $"🤛{message.From!.FirstName} {message.From!.LastName}";

			var sentMessage = await API.SendMessage(messageText,
				message.Chat.Id,
				ParseMode.None,
				0,
				_kulakButton,
				false);

			_kulakState[message.Chat.Id] = new KulakState
			{
				MessageId = sentMessage.Id,
				PressedBy = [message.From!.Id],
				StartedAt = DateTime.Now,
				MessageText = messageText
			};
		}

		private async Task SendNewSorry(Message message)
		{
			var messageText = $"✋{message.From!.FirstName} {message.From!.LastName}";

			var sentMessage = await API.SendMessage(messageText,
				message.Chat.Id,
				ParseMode.None,
				0,
				_sorryButton,
				false);

			_sorryState[message.Chat.Id] = new KulakState
			{
				MessageId = sentMessage.Id,
				PressedBy = [message.From!.Id],
				StartedAt = DateTime.Now,
				MessageText = messageText
			};
		}

		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			if (callback.Message is null || callback.Data != "kulak" && callback.Data != "sorry" || callback.Message.From == null)
				return "???";

			if (callback.Data == "kulak" && _kulakState.TryGetValue(callback.Message.Chat.Id, out var existingKulak))
			{
				if (existingKulak.StartedAt < DateTime.Now.AddSeconds(-seconds))
				{
					return "🤛";
				}

				if (existingKulak.PressedBy.Contains(callback.From.Id))
				{
					return "🤛";
				}

				existingKulak.PressedBy = [.. existingKulak.PressedBy, callback.From.Id];
				existingKulak.MessageText += $"\n🤛{callback.From!.FirstName} {callback.From!.LastName}";
				await API.EditMessage(callback.Message.Chat.Id, existingKulak.MessageId, existingKulak.MessageText, _kulakButton);

				return "🤛";
			}

			if (callback.Data == "sorry" && _sorryState.TryGetValue(callback.Message.Chat.Id, out var existingSorry))
			{
				if (existingSorry.StartedAt < DateTime.Now.AddSeconds(-seconds))
				{
					return "✋";
				}

				if (existingSorry.PressedBy.Contains(callback.From.Id))
				{
					return "✋";
				}

				existingSorry.PressedBy = [.. existingSorry.PressedBy, callback.From.Id];
				existingSorry.MessageText += $"\n✋{callback.From!.FirstName} {callback.From!.LastName}";
				await API.EditMessage(callback.Message.Chat.Id, existingSorry.MessageId, existingSorry.MessageText, _sorryButton);
			}

			return "✋";
		}
	}
}
