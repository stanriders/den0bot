// den0bot (c) StanR 2024 - MIT License
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
        private readonly Regex regex = new(@".+\/\/(?>twitter|x)\.com\/(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Text))
				return;

            Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
                var tail = regexMatch.Groups.Values.ToArray()[1];

                await API.SendMessage($"https://vxtwitter.com/{tail}", message.Chat.Id, replyToId: message.MessageId, disablePreview: false);
            }
		}
	}
}
