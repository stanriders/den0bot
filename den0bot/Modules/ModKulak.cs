// den0bot (c) StanR 2026 - MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using den0bot.Types;
using den0bot.Types.Answers;
using Microsoft.Extensions.Caching.Memory;
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

		private readonly IMemoryCache _masteryCache;

		private readonly InlineKeyboardMarkup _kulakButton = new(
			new[] { new InlineKeyboardButton("🤛") { CallbackData = "kulak" },
			}
		);

		private readonly InlineKeyboardMarkup _sorryButton = new(
			new[] { new InlineKeyboardButton("✋") { CallbackData = "sorry" },
			}
		);

		private const int seconds = 60 * 3;

		public ModKulak(ILogger<IModule> logger, IMemoryCache masteryCache) : base(logger)
		{
			_masteryCache = masteryCache;
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
				new Command
				{
					Name = "mastery",
					ActionAsync = Mastery,
					Reply = true
				}
			});
		}

		private async Task<ICommandAnswer> Mastery(Message message)
		{
			if (message?.From == null)
				return null;

			var key = $"mastery_{message.From.Id}";
			var keyPost = $"mastery_{message.From.Id}_lastpost";

			if (_masteryCache.TryGetValue(key, out string mastery))
			{
				if (_masteryCache.TryGetValue(keyPost, out _))
				{
					await API.RemoveMessage(message.Chat.Id, message.Id);
					return new EmptyCommandAnswer();
				}

				_masteryCache.Set(keyPost, "", TimeSpan.FromMinutes(1));
				return new StickerCommandAnswer(mastery);
			}

			var stickers = new List<(string, double)>
			{
				("CAACAgIAAxkBAAERkAZqW2sdZOk3pnEniY6y3UoFe7hFTwACq54AAtlw2Up4ylO2YVow8T0E", 60), // none
				("CAACAgIAAxkBAAERj_ZqW2m7K91mA_vG4NpjfDoW9pDq8gAC7acAAsrT2UrVVTZTOIrrBT0E", 50), // mastery 4
				("CAACAgIAAxkBAAERj_hqW2m9IPyyY0BVTEf4_UhwurY3-wAC85oAAsSv2UpHbFEvb7qBaj0E", 50), // mastery 5
				("CAACAgIAAxkBAAERj_pqW2nBUBa4NsyLeRiS_gLhd0cjrAAC1p4AAgu04ErXKm3N-dTXhj0E", 20), // mastery 6
				("CAACAgIAAxkBAAERj_xqW2nCi5KTEkvBxzaLvuoyJGLUyAACL6IAAknP2UqkSOiPNNZWpz0E", 20), // mastery 7
				("CAACAgIAAxkBAAERj_5qW2nFxCNtCgUNBAvg3kXkjEPaYQACoJ8AAiJY4UrPjYxUJtcxpT0E", 10), // mastery 8
				("CAACAgIAAxkBAAERkAABaltpxi2AegGqXBF7LxA8WqTWULoAAgKnAAKTldlK350lJoXmuPQ9BA", 5),// mastery 9
				("CAACAgIAAxkBAAERkAJqW2nKfoxqBnZFzRft94oW5YKh-wACZKkAAsCk4Ep_6IUnajiAIj0E", 5),  // mastery 10
				("CAACAgIAAxkBAAERkARqW2nL0Y_JN53ma-e-KF1_Y3W66QACk7AAAtcT4Epn0aoICos3fz0E", 10), // mastery question
				("CAACAgIAAxkBAAERj_RqW2m2YEndvXOGdo4BBWq512241wACL6UAAqQH2ErXNhc8HuSCcD0E", 5)   // mastery wood
			};

			double totalWeight = 0;

			foreach (var (_, weight) in stickers)
				totalWeight += weight;

			double roll = Random.Shared.NextDouble() * totalWeight;

			foreach (var (item, weight) in stickers)
			{
				roll -= weight;

				if (roll <= 0)
				{
					_masteryCache.Set(key, item, TimeSpan.FromSeconds(seconds));
					_masteryCache.Set(keyPost, "", TimeSpan.FromMinutes(1));
					return new StickerCommandAnswer(item);
				}
			}

			_masteryCache.Set(key, stickers[0].Item1, TimeSpan.FromSeconds(seconds));
			_masteryCache.Set(keyPost, "", TimeSpan.FromMinutes(1));
			return new StickerCommandAnswer(stickers[0].Item1);
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
