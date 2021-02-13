
using System.Text.RegularExpressions;

namespace den0bot.Modules.Osu.Parsers
{
	public class MatchLinkData
	{
		public ulong Id { get; set; }
	}

	public static class MatchLinkParser
	{
		private static readonly Regex linkRegex = new(@"(?>https?:\/\/)?osu\.ppy\.sh\/community\/matches\/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static MatchLinkData Parse(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				Match regexMatch = linkRegex.Match(text);
				if (regexMatch.Groups.Count > 1 && uint.TryParse(regexMatch.Groups[1].Value, out var id))
					return new MatchLinkData {Id = id};
			}

			return null;
		}
	}
}
