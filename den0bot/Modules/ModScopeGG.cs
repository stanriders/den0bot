// den0bot (c) StanR 2024 - MIT License
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	internal partial class ModScopeGG : IModule, IReceiveAllMessages
	{
		private readonly ILogger<IModule> logger;

		public ModScopeGG(ILogger<IModule> logger) : base(logger)
		{
			this.logger = logger;
		}

		[GeneratedRegex(@"(https:\/\/app\.scope\.gg([\w/]*[\w/])?)")]
		private static partial Regex ScopeLinkRegex();

		[GeneratedRegex(@"(https:\/\/hl\.xplay\.cloud\/video([a-zA-Z0-9/]*[a-zA-Z0-9/])?.mp4)")]
		private static partial Regex ClipLinkRegex();

		[GeneratedRegex(@"""clipTitle"":\s+?""(.+?)"",")]
		private static partial Regex ClipTitleCodeRegex();
		
		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Text))
				return;

			var regexMatch = ScopeLinkRegex().Match(message.Text);
			if (regexMatch.Success)
			{
				using var client = new HttpClient();
				var page = await client.GetStringAsync(regexMatch.Value);
				
				var match = ClipLinkRegex().Match(page);
				var videoLink = match.Value;

				if (Uri.TryCreate(videoLink, UriKind.Absolute, out var videoUri))
				{
					var clipTitleCodeMatch = ClipTitleCodeRegex().Match(page);
					string caption;
				if (clipTitleCodeMatch.Success)
				{
					Regex clipTitleRegex = new Regex(ClipTitlePattern(clipTitleCodeMatch.Groups[1].Value));
					var clipTitleMatch = clipTitleRegex.Match(page);
					caption = clipTitleMatch.Success ? clipTitleMatch.Groups[1].Value : "VAC";
				}
				else
				{
					caption = "VAC";
				}

					if (!await API.SendVideo(videoLink, message.Chat.Id, caption, message.MessageId))
					{
						// telegram couldn't download the video by itself, stream it manually
						var video = await client.GetStreamAsync(videoUri);

						await API.SendVideo(video, message.Chat.Id, caption, message.MessageId);
					}
				}
				else
				{
					logger.LogError("Invalid scope.gg link {Link}", videoLink);
				}
			}
		}

		private static string ClipTitlePattern(string clipTitleCode)
		{
			return @"""" + Regex.Escape(clipTitleCode) + @""":\s+?""(.+?)"",";
		}  
	}
}