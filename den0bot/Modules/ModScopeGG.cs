using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	internal class ModScopeGG : IModule, IReceiveAllMessages
	{
		private readonly Regex scopeLinkRegex = new(@"(https:\/\/app\.scope\.gg([\w/]*[\w/])?)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex clipLinkRegex = new(@"(https:\/\/mediacdn\.allstar\.gg([a-zA-Z0-9/]*[a-zA-Z0-9/])?)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex clipLinkReplaceRegex = new(@"(?>thumbs|og)");
		private readonly Regex clipTitleRegex = new(@"clipTitle.*""(.*)""");

		public ModScopeGG(ILogger<IModule> logger) : base(logger)
		{
		}

		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Text))
				return;

			Match regexMatch = scopeLinkRegex.Match(message.Text);
			if (regexMatch.Success)
			{
				HttpClient client = new HttpClient();
				string page = await client.GetStringAsync(regexMatch.Value);
				
				Match match = clipLinkRegex.Match(page);
				string video = string.Concat(clipLinkReplaceRegex.Replace(match.Value, "clips"), ".mp4");
				
				string caption = clipTitleRegex.Match(page).Value;

				await API.SendVideo(video, message.Chat.Id, caption, message.MessageId);
			}
		}
	}
}