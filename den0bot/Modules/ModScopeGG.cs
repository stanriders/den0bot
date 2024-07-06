using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	internal partial class ModScopeGG(ILogger<IModule> logger) : IModule(logger), IReceiveAllMessages
	{
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
				var client = new HttpClient();
				var page = await client.GetStringAsync(regexMatch.Value);
				
				var match = ClipLinkRegex().Match(page);
				var video = string.Concat(ClipLinkReplaceRegex().Replace(match.Value, "clips"), ".mp4");

				var clipMatch = ClipTitleRegex().Match(page);
				var caption = clipMatch.Success ? clipMatch.Groups[1].Value : "VAC";

				await API.SendVideo($"{video}?{Random.Shared.Next()}", message.Chat.Id, caption, message.MessageId);
			}
		}

	}
}