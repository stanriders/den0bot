// den0bot (c) StanR 2018 - MIT License

namespace den0bot.Locales
{
	public interface ILocale
	{
		bool Contains(string key);
		string GetLocalizedString(string key);
		string NewMemberGreeting(string name, long userID);
	}
}
