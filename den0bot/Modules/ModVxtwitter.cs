// den0bot (c) StanR 2025 - MIT License
using System.Linq;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using den0bot.Types;
using Microsoft.Extensions.Logging;

namespace den0bot.Modules
{
	internal class ModVxtwitter(ILogger<IModule> logger) : IModule(logger), IReceiveAllMessages
	{
		private readonly Regex twitterRegex = new(@".+\/\/(?>twitter|x)\.com\/(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex instagramRegex = new(@".+\/\/(?>www\.)?instagram\.com\/(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly Regex tiktokRegex = new(@".+\/\/(?>\w+\.)?tiktok\.com\/.+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Text))
				return;

			var twitterRegexMatch = twitterRegex.Match(message.Text);
			if (twitterRegexMatch.Groups.Count > 1)
			{
				var tail = twitterRegexMatch.Groups.Values.ToArray()[1];

				await API.SendMessage($"https://vxtwitter.com/{tail}", message.Chat.Id, replyToId: message.MessageId, disablePreview: false);
				return;
			}

			var instagramRegexMatch = instagramRegex.Match(message.Text);
			if (instagramRegexMatch.Groups.Count > 1)
			{
				var tail = instagramRegexMatch.Groups.Values.ToArray()[1];

				await API.SendMessage($"https://kkinstagram.com/{tail}", message.Chat.Id, replyToId: message.MessageId, disablePreview: false);
				return;
			}

			if (tiktokRegex.IsMatch(message.Text))
			{
				await API.SendMessage(message.Text.Replace("tiktok.com", "vxtiktok.com"), message.Chat.Id, replyToId: message.MessageId, disablePreview: false);
			}
		}
	}
}