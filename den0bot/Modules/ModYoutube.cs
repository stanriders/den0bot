﻿// den0bot (c) StanR 2020 - MIT License
using System;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Util;
using Newtonsoft.Json;

namespace den0bot.Modules
{
	// check certain youtube channel and post new highscores to all chats
	class ModYoutube : IModule
	{
		private DateTime nextCheck;
		private readonly bool isEnabled = false;

		private readonly string api_key = Config.Params.GoogleAPIToken;
		private const string channel_id = "UCt1GKXk_zkcBUEwAeXZ43RA";  // circle people again
		private const double check_interval = 1.0; // minutes
		private const int default_score_amount = 3;

		public ModYoutube()
		{
			nextCheck = DateTime.Now;

			AddCommands(new []
			{
				new Command()
				{
					Name = "disableannouncements",
					IsAdminOnly = true,
					Action = (msg) =>
					{
						Database.ToggleAnnouncements(msg.Chat.Id, false);
						return "Понял, вырубаю";
					}
				},
				new Command()
				{
					Name = "enableannouncements",
					IsAdminOnly = true,
					Action = (msg) =>
					{
						Database.ToggleAnnouncements(msg.Chat.Id, true);
						return "Понял, врубаю";
					}
				},
				new Command
				{
					Name = "newscores",
					ActionAsync = (msg) =>
					{
						try
						{
							int amount = int.Parse(msg.Text.Remove(0, 10));
							if (amount > 20)
								return GetLastScores(default_score_amount);
							else
								return GetLastScores(amount);
						}
						catch (Exception)
						{
							return GetLastScores(default_score_amount);
						}
					}
				}
			});

			if (string.IsNullOrEmpty(api_key))
			{
				Log.Error("API Key is not defined!");
			}
			else
			{
				isEnabled = true;
				Log.Debug("Enabled");
			}
		}

		public override void Think()
		{
			if (isEnabled && nextCheck < DateTime.Now)
			{
				Update(nextCheck);
				nextCheck = DateTime.Now.AddMinutes(check_interval);
			}
		}

		private async void Update(DateTime lastChecked)
		{
			try
			{
				lastChecked = lastChecked.AddMinutes(-check_interval);

				string request = string.Format("https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" +
												"&key={0}" + "&fields={1}" + "&publishedAfter={2}" + "&channelId={3}",
												api_key,
												Uri.EscapeDataString("items(contentDetails/upload,snippet/title)"),
												Uri.EscapeDataString(lastChecked.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ")),
												channel_id);

				var data = await Web.DownloadString(request);

				dynamic items = JsonConvert.DeserializeObject(data);

				foreach (var vid in items.items)
				{
					await API.SendMessageToAllChats(
						$"❗️ {vid.snippet.title}{Environment.NewLine}http://youtu.be/{vid.contentDetails.upload.videoId}");
				}
			}
			catch (Exception ex) { Log.Error(ex.InnerMessageIfAny()); }
		}

		private async Task<string> GetLastScores(int amount)
		{
			if (!isEnabled)
				return "Сегодня без скоров";

			string result = string.Empty;
			try
			{
				string request = string.Format("https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" +
												"&maxResults={0}" + "&key={1}" + "&fields={2}" + "&channelId={3}",
												amount,
												api_key,
												Uri.EscapeDataString("items(contentDetails/upload,snippet/title)"),
												channel_id);

				var data = await Web.DownloadString(request);

				dynamic items = JsonConvert.DeserializeObject(data);

				for (int i = 0; i < 3; i++)
				{
					var vid = items.items[i];
					result += $"{vid.snippet.title}\nhttp://youtu.be/{vid.contentDetails.upload.videoId}";
				}
			}
			catch (Exception ex) { Log.Error(ex.InnerMessageIfAny()); }

			return result;
		}
	}
}
