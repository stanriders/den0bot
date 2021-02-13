// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.WebAPI;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Types;
using den0bot.Util;
using FFmpeg.NET;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Parsers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace den0bot.Modules.Osu
{
	public class ModBeatmap : IModule, IReceiveAllMessages, IReceiveCallback
	{
		private readonly MemoryCache sentMapsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep maps in cache

		private readonly InlineKeyboardMarkup buttons = new(
			new[] {new InlineKeyboardButton {Text = "Preview", CallbackData = "preview"},}
		);

		private class RebalanceMap
		{
			public int? BeatmapSetId { get; set; }
			public string Title { get; set; }
			public double Stars { get; set; }
			public double[] PP { get; set; }
		}

		public ModBeatmap()
		{
			AddCommand(new Command
			{
				Name = "newppmap",
				ActionAsync = GetRebalancePp,
				Reply = true
			});
			Log.Debug("Enabled");
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
			{
				var beatmapLinkData = BeatmapLinkParser.Parse(message.Text);
				if (beatmapLinkData != null)
				{
					Beatmap map = null;
					if (beatmapLinkData.IsBeatmapset)
					{
						var set = await WebApiHandler.MakeApiRequest(new GetBeatmapSet(beatmapLinkData.ID));
						if (set?.Beatmaps.Count > 0)
							map = set.Beatmaps.Last();
					}
					else
					{
						map = await WebApiHandler.MakeApiRequest(new GetBeatmap(beatmapLinkData.ID));
					}

					if (map != null)
					{
						var sentMessage = await API.SendPhoto(map.BeatmapSet.Covers.Cover2X, 
							message.Chat.Id, 
							map.GetFormattedMapInfo(beatmapLinkData.Mods),
							Telegram.Bot.Types.Enums.ParseMode.Html, 
							0, 
							buttons);

						if (sentMessage != null)
						{
							// we only store mapset id to spare the memory a bit
							sentMapsCache.Add(sentMessage.MessageId.ToString(), map.BeatmapSet.Id,
								DateTimeOffset.Now.AddDays(days_to_keep_messages));

							ChatBeatmapCache.StoreMap(message.Chat.Id, map.Id);
						}
					}
				}
			}
		}

		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			if (sentMapsCache.Contains(callback.Message.MessageId.ToString()) && callback.Data == "preview")
			{
				await API.AnswerCallbackQuery(callback.Id, "Ща всё будет");
				var mapsetId = sentMapsCache.Remove(callback.Message.MessageId.ToString()) as uint?;

				try
				{
					var data = await Web.DownloadBytes($"https://b.ppy.sh/preview/{mapsetId}.mp3");
					await File.WriteAllBytesAsync($"./{mapsetId}.mp3", data);
				}
				catch (Exception e)
				{
					Log.Error(e.InnerMessageIfAny());
					return string.Empty;
				}

				await new Engine("ffmpeg")
					.ConvertAsync(new MediaFile($"./{mapsetId}.mp3"), new MediaFile($"./{mapsetId}.ogg"));

				using (FileStream fs = File.Open($"./{mapsetId}.ogg", FileMode.Open, FileAccess.Read))
					await API.SendVoice(new InputOnlineFile(fs), callback.Message.Chat.Id, replyTo: callback.Message.MessageId, duration: 10);

				File.Delete($"./{mapsetId}.mp3");
				File.Delete($"./{mapsetId}.ogg");

				await API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId,
					callback.Message.Caption, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
			}

			return string.Empty;
		}

		private async Task<ICommandAnswer> GetRebalancePp(Message msg)
		{
			if (!string.IsNullOrEmpty(msg.Text))
			{
				var beatmapLinkData = BeatmapLinkParser.Parse(msg.Text);
				if (beatmapLinkData != null && !beatmapLinkData.IsBeatmapset)
				{
					var json = new { Map = beatmapLinkData.ID.ToString(), Mods = beatmapLinkData.Mods.ToArray() };
					try
					{
						var mapJson = await Web.PostJson("https://newpp.stanr.info/api/CalculateMap",
							JsonConvert.SerializeObject(json));
						if (!string.IsNullOrEmpty(mapJson))
						{
							var map = JsonConvert.DeserializeObject<RebalanceMap>(mapJson);
							if (map != null)
							{
								ChatBeatmapCache.StoreMap(msg.Chat.Id, beatmapLinkData.ID);
								return new ImageCommandAnswer
								{
									Image = $"https://assets.ppy.sh/beatmaps/{map.BeatmapSetId}/covers/card@2x.jpg",
									Caption = $"{map.Title}\n{map.Stars:F2}*\n100% - {map.PP[10]}pp | 98% - {map.PP[8]}pp | 95% - {map.PP[5]}pp"
								};
							}
						}
					}
					catch (Exception)
					{
						return Localization.GetAnswer("generic_fail", msg.Chat.Id);
					}
				}
			}

			return Localization.GetAnswer("generic_badrequest", msg.Chat.Id);
		}
	}
}
