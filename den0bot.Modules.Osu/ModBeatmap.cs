// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Osu.API.Requests;
using den0bot.Modules.Osu.Osu.Types;
using den0bot.Util;
using FFmpeg.NET;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace den0bot.Modules.Osu
{
	public class ModBeatmap : IModule, IReceiveAllMessages, IReceiveCallback
	{
		private readonly Regex regex = new Regex(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/([b,s]|(?>beatmaps)|(?>beatmapsets))\/(\d+\/?\#osu\/)?(\d+)?\/?(?>[&,?].=\d)?\s?(?>\+(\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly MemoryCache sentMapsCache = MemoryCache.Default;
		private const int days_to_keep_messages = 1; // how long do we keep maps in cache

		private readonly InlineKeyboardMarkup buttons = new InlineKeyboardMarkup(
			new[] {new InlineKeyboardButton {Text = "Preview", CallbackData = "preview"},}
		);

		public async Task ReceiveMessage(Message message)
		{
			Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
				List<Group> regexGroups = regexMatch.Groups.OfType<Group>().Where(x => (x != null) && (x.Length > 0)).ToList();

				bool isNew = regexGroups[1].Value == "beatmapsets"; // are we using new website or not
				bool isSet = false;
				uint beatmapId = 0;
				Mods mods = Mods.None;

				if (isNew)
				{
					if (regexGroups[2].Value.Contains("#osu/"))
					{
						beatmapId = uint.Parse(regexGroups[3].Value);
						if (regexGroups.Count > 4)
							mods = ConvertToMods(regexGroups[4].Value);
					}
					else
					{
						isSet = true;
						beatmapId = uint.Parse(regexGroups[2].Value);
						if (regexGroups.Count > 3)
							mods = ConvertToMods(regexGroups[3].Value);
					}
				}
				else
				{ 
					if(regexGroups[1].Value == "s")
						isSet = true;

					beatmapId = uint.Parse(regexGroups[2].Value);
					if (regexGroups.Count > 3)
						mods = ConvertToMods(regexGroups[3].Value);
				}

				Map map = null;
				if (isSet)
				{
					List<Map> set = await Osu.WebApi.MakeAPIRequest(new GetBeatmapSet
					{
						ID = beatmapId
					});

					if (set?.Count > 0)
						map = set.Last();
				}
				else
				{
					map = await Osu.WebApi.MakeAPIRequest(new GetBeatmap
					{
						ID = beatmapId,
						Mods = mods
					});
				}

				if (map != null)
				{
					var sentMessage = await API.SendPhoto(map.Thumbnail, message.Chat.Id, map.GetFormattedMapInfo(mods), Telegram.Bot.Types.Enums.ParseMode.Html, 0, buttons);
					if (sentMessage != null)
					{
						// we only store mapset id to spare the memory a bit
						sentMapsCache.Add(sentMessage.MessageId.ToString(), map.BeatmapSetID, DateTimeOffset.Now.AddDays(days_to_keep_messages));
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

				using (var web = new WebClient())
				{
					try
					{
						var data = web.DownloadData($"https://b.ppy.sh/preview/{mapsetId}.mp3");
						File.WriteAllBytes($"./{mapsetId}.mp3", data);
					}
					catch (Exception e)
					{
						Log.Error(e.InnerMessageIfAny());
						return string.Empty;
					}

					await new Engine("ffmpeg").ConvertAsync(new MediaFile($"./{mapsetId}.mp3"),
						new MediaFile($"./{mapsetId}.ogg"));

					using (FileStream fs = File.Open($"./{mapsetId}.ogg", FileMode.Open, FileAccess.Read))
						await API.SendVoice(new InputOnlineFile(fs), callback.Message.Chat.Id, replyTo: callback.Message.MessageId);

					File.Delete($"./{mapsetId}.mp3");
					File.Delete($"./{mapsetId}.ogg");
				}

				await API.EditMediaCaption(callback.Message.Chat.Id, callback.Message.MessageId, callback.Message.Caption, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
			}
			return string.Empty;
		}

		private static Mods ConvertToMods(string mods)
		{
			if (Enum.TryParse(mods, true, out Mods result) || string.IsNullOrEmpty(mods) || mods.Length > 36) // every mod combination possible
				return result;
			else
			{
				StringBuilder builder = new StringBuilder(mods.Length * 2);
				bool secondChar = false;
				foreach (char c in mods)
				{
					builder.Append(c);
					if (secondChar)
					{
						builder.Append(',');
						builder.Append(' ');
					}
					secondChar = !secondChar;
				}
				builder.Remove(builder.Length - 2, 2);
				Enum.TryParse(builder.ToString(), true, out result);
				return result;
			}
		}
	}
}
