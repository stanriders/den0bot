// den0bot (c) StanR 2024 - MIT License
//#define PARSE_PHOTOS
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.WebAPI.Requests.V2;
using den0bot.Types;
using den0bot.Util;
using FFmpeg.NET;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Parsers;
using den0bot.Modules.Osu.Types;
#if PARSE_PHOTOS
using IronOcr;
#endif
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace den0bot.Modules.Osu
{
	public class ModBeatmap : OsuModule, IReceiveAllMessages, IReceiveCallbacks
	{
		private readonly ILogger<IModule> logger;

		private readonly InlineKeyboardMarkup buttons = new(
			new[] {new InlineKeyboardButton("Preview") {CallbackData = "preview"},}
		);

		public async Task ReceiveMessage(Message message)
		{
#if PARSE_PHOTOS
			if (message.Photo is not null)
			{
				try
				{
					var photo = message.Photo.Last();
					var (name, mappedBy) = await ParseScoreScreenshot(photo);
					if (name is not null)
					{
						var query = name;
						if (mappedBy is not null)
							query += $" creator={mappedBy}";

						var sets = await new BeatmapSetSearch(query).Execute();
						if (sets.Length > 0)
						{
							var map = sets[0].Beatmaps.FirstOrDefault(x => name.Contains(x.Version));
							if (map is null)
								map = sets[0].Beatmaps.OrderByDescending(x => x.StarRating).First();

							map.BeatmapSet = sets[0];

							await SendMapInfo(message.Chat.Id, map, LegacyMods.NM, true);
						}
					}
				}
				catch (Exception e)
				{
					logger.LogError(e.InnerMessageIfAny());
				}
				return;
			}
#endif
			if (!string.IsNullOrEmpty(message.Text))
			{
				var beatmapLinkData = BeatmapLinkParser.Parse(message.Text);
				if (beatmapLinkData != null)
				{
					Beatmap map = null;
					if (beatmapLinkData.IsBeatmapset)
					{
						var set = await new GetBeatmapSet(beatmapLinkData.ID).Execute();
						if (set?.Beatmaps.Count > 0)
							map = set.Beatmaps.Last();
					}
					else
					{
						map = await new GetBeatmap(beatmapLinkData.ID).Execute();
					}

					await SendMapInfo(message.Chat.Id, map, beatmapLinkData.Mods);
				}
			}
		}

		public async Task<string> ReceiveCallback(CallbackQuery callback)
		{
			var sentMap = ChatBeatmapCache.GetSentMap(callback.Message.MessageId);
			if (callback.Data == "preview" && sentMap?.BeatmapSetId is not null)
			{
				await API.AnswerCallbackQuery(callback.Id, "Ща всё будет");

				try
				{
					var data = await Web.DownloadBytes($"https://b.ppy.sh/preview/{sentMap.BeatmapSetId}.mp3");
					await File.WriteAllBytesAsync($"./{sentMap.BeatmapSetId}.mp3", data);

					await new Engine("ffmpeg")
						.ConvertAsync(new FFmpeg.NET.InputFile($"./{sentMap.BeatmapSetId}.mp3"), new OutputFile($"./{sentMap.BeatmapSetId}.ogg"), CancellationToken.None);

					await using (FileStream fs = File.Open($"./{sentMap.BeatmapSetId}.ogg", FileMode.Open, FileAccess.Read))
						await API.SendVoice(new InputFileStream(fs), callback.Message.Chat.Id, replyToId: callback.Message.MessageId, duration: 10);

					File.Delete($"./{sentMap.BeatmapSetId}.mp3");
					File.Delete($"./{sentMap.BeatmapSetId}.ogg");
				}
				catch (Exception e)
				{
					logger.LogError(e.InnerMessageIfAny());
					return string.Empty;
				}

				await API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId,
					callback.Message.Caption, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
			}

			return string.Empty;
		}

#if PARSE_PHOTOS
		private async Task<(string? name, string? mappedBy)> ParseScoreScreenshot(PhotoSize photo)
		{
			var fileName = $"cache/{RNG.Next()}_{photo.FileId}";
			await API.DownloadFile(photo.FileId, fileName);

			var ocr = new IronTesseract {Configuration = {ReadBarCodes = false}, Language = OcrLanguage.EnglishFast};
			var result = await ocr.ReadAsync(fileName);

			File.Delete(fileName);

			var lines = result.Blocks.SelectMany(x => x.Lines)
				.SelectMany(x=> x.Text.Split("\n"))
				.Select(x=> x.Trim())
				.ToArray();

			if (lines.Length < 2 || !lines.Any(x => x.Contains("Beatmap by")))
				return (null, null);

			string mappedBy;
			if (lines[1].Any(x => !char.IsLetterOrDigit(x) && !char.IsWhiteSpace(x)))
				mappedBy = null;
			else
				mappedBy = lines[1].Replace("Beatmap by ", "").Trim();

			return (lines[0], mappedBy);
		}
#endif

		private async Task SendMapInfo(long chatId, Beatmap map, Mod[] mods, bool includeName = false)
		{
			if (map != null)
			{
				var sentMessage = await API.SendPhoto(map.BeatmapSet.Covers.Cover2X,
					chatId,
					await map.GetFormattedMapInfo(mods, includeName),
					Telegram.Bot.Types.Enums.ParseMode.Html,
					replyMarkup: buttons);

				if (sentMessage != null)
				{
					var cachedBeatmap = new ChatBeatmapCache.CachedBeatmap
					{
						BeatmapId = map.Id,
						BeatmapSetId = map.BeatmapSetId
					};

					ChatBeatmapCache.StoreSentMap(sentMessage.MessageId, cachedBeatmap);
					ChatBeatmapCache.StoreLastMap(chatId, cachedBeatmap);
				}
			}
		}

		public ModBeatmap(ILogger<IModule> logger) : base(logger)
		{
			this.logger = logger;
		}
	}
}
