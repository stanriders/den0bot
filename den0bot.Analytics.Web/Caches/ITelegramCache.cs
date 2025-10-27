// den0bot (c) StanR 2025 - MIT License
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = den0bot.Analytics.Data.Types.User;

namespace den0bot.Analytics.Web.Caches
{
	public interface ITelegramCache
	{
		Task<User?> GetUser(long? chatId, long userId);
		Task<ChatFullInfo?> GetChat(long chatId);
		Task<string?> GetChatImage(long chatId, string fileId);
		Task<string?> GetAvatar(long userId);
	}
}
