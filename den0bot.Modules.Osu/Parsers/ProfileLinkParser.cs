// den0bot (c) StanR 2024 - MIT License
using System.Text.RegularExpressions;

namespace den0bot.Modules.Osu.Parsers
{
	public class ProfileLinkData
	{
		public string Id { get; set; } = null!;
	}

	public static class ProfileLinkParser
	{
		private static readonly Regex linkRegex = new(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/u(?>sers)?\/(\d+|\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static ProfileLinkData? Parse(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				Match regexMatch = linkRegex.Match(text);
				if (regexMatch.Groups.Count > 1)
					return new ProfileLinkData {Id = regexMatch.Groups[1].Value};
			}

			return null;
		}
	}
}
