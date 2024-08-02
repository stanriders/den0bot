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

		[GeneratedRegex(@"(https:\/\/mediacdn\.allstar\.gg([a-zA-Z0-9/]*[a-zA-Z0-9/])?)")]
		private static partial Regex ClipLinkRegex();

		[GeneratedRegex(@"(?>thumbs|og)")]
		private static partial Regex ClipLinkReplaceRegex();

		[GeneratedRegex(@"""clipTitle"":""(.+?)"",")]
		private static partial Regex ClipTitleRegex();
		
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
				var videoLink = string.Concat(ClipLinkReplaceRegex().Replace(match.Value, "clips"), ".mp4");

				if (Uri.TryCreate(videoLink, UriKind.RelativeOrAbsolute, out var videoUri))
				{
					var clipMatch = ClipTitleRegex().Match(page);
					var caption = clipMatch.Success ? clipMatch.Groups[1].Value : "VAC";

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

	}
}