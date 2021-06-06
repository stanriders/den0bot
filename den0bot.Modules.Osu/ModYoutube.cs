// den0bot (c) StanR 2021 - MIT License
using System;
using System.Linq;
using System.Threading.Tasks;
using den0bot.DB;
using den0bot.Modules.Osu.Util;
using den0bot.Types;
using den0bot.Types.Answers;
using den0bot.Util;
using Newtonsoft.Json;
using Serilog;

namespace den0bot.Modules.Osu
{
	// check a youtube channel and post new videos to all chats
	internal class ModYoutube : IModule
	{
		private DateTime nextCheck = DateTime.Now;

		private const double check_interval = 1.0; // minutes
		private const int default_score_amount = 3;

		public ModYoutube()
		{
			AddCommands(new []
			{
				new Command
				{
					Name = "disableannouncements",
					IsAdminOnly = true,
					ActionAsync = async (msg) =>
					{
						await using(var db = new Database())
						{
							var chat = db.Chats.First(x=> x.Id == msg.Chat.Id);
							chat.DisableAnnouncements = false;
							await db.SaveChangesAsync();

							return new TextCommandAnswer("Понял, вырубаю");
						}
					}
				},
				new Command
				{
					Name = "enableannouncements",
					IsAdminOnly = true,
					ActionAsync = async (msg) =>
					{
						await using(var db = new Database())
						{
							var chat = db.Chats.First(x=> x.Id == msg.Chat.Id);
							chat.DisableAnnouncements = true;
							await db.SaveChangesAsync();

							return new TextCommandAnswer("Понял, врубаю");
						}
					}
				},
				new Command
				{
					Name = "newscores",
					ActionAsync = async (msg) =>
					{
						try
						{
							int amount = int.Parse(msg.Text.Remove(0, 10));
							if (amount > 20)
								return new TextCommandAnswer(await GetLastScores(default_score_amount));
							else
								return new TextCommandAnswer(await GetLastScores(amount));
						}
						catch (Exception)
						{
							return new TextCommandAnswer(await GetLastScores(default_score_amount));
						}
					}
				}
			});
		}

		public override bool Init()
		{
			if (string.IsNullOrEmpty(Config.Params.GoogleAPIToken))
			{
				Log.Error("API Key is not defined!");
				return false;
			}

			if (string.IsNullOrEmpty(Config.Params.YoutubeChannelId))
			{
				Log.Error("Youtube channel id is not defined!");
				return false;
			}

			return base.Init();
		}

		public override void Think()
		{
			if (nextCheck < DateTime.Now)
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

				string request = "https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" +
				                 $"&key={Config.Params.GoogleAPIToken}" +
				                 $"&fields={Uri.EscapeDataString("items(contentDetails/upload,snippet/title)")}" +
				                 $"&publishedAfter={Uri.EscapeDataString(lastChecked.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ"))}" +
				                 $"&channelId={Config.Params.YoutubeChannelId}";

				var data = await Web.DownloadString(request);
				if (!string.IsNullOrEmpty(data))
				{
					dynamic items = JsonConvert.DeserializeObject(data);

					foreach (var vid in items.items)
					{
						// this needs remaking with proper per-chat subscriptions 
						//await API.SendMessageToAllChats(
						//	$"❗️ {vid.snippet.title}{Environment.NewLine}http://youtu.be/{vid.contentDetails.upload.videoId}");
					}
				}
			}
			catch (Exception ex) { Log.Error(ex.InnerMessageIfAny()); }
		}

		private async Task<string> GetLastScores(int amount)
		{
			string result = string.Empty;
			try
			{
				string request = "https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails" +
				                 $"&maxResults={amount}" + 
				                 $"&key={Config.Params.GoogleAPIToken}" +
				                 $"&fields={Uri.EscapeDataString("items(contentDetails/upload,snippet/title)")}" +
				                 $"&channelId={Config.Params.YoutubeChannelId}";

				var data = await Web.DownloadString(request);
				if (!string.IsNullOrEmpty(data))
				{
					dynamic items = JsonConvert.DeserializeObject(data);

					for (int i = 0; i < 3; i++)
					{
						var vid = items.items[i];
						result += $"{vid.snippet.title}\nhttp://youtu.be/{vid.contentDetails.upload.videoId}";
					}
				}
			}
			catch (Exception ex) { Log.Error(ex.InnerMessageIfAny()); }

			return result;
		}
	}
}
