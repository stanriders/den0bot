// den0bot (c) StanR 2022 - MIT License
using System.Linq;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using den0bot.Types;

namespace den0bot.Modules
{
	internal class ModVxtwitter : IModule, IReceiveAllMessages
	{
        private readonly Regex regex = new(@".+\/\/twitter\.com\/(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public async Task ReceiveMessage(Message message)
		{
			if (string.IsNullOrEmpty(message.Text))
				return;

            Match regexMatch = regex.Match(message.Text);
			if (regexMatch.Groups.Count > 1)
			{
                var tail = regexMatch.Groups.Values.ToArray()[1];

                await API.SendMessage($"https://vxtwitter.com/{tail}", message.Chat.Id, replyToId: message.MessageId);
            }
		}
	}
}
