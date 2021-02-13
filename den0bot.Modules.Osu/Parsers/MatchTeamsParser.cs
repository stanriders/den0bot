
using System.Text.RegularExpressions;

namespace den0bot.Modules.Osu.Parsers
{
	public class MatchTeamNames
	{
		public string RedTeam { get; set; }
		public string BlueTeam { get; set; }
	}

	public static class MatchTeamsParser
	{
		private static readonly Regex regex = new(@".+: ?\((.+)\) ?vs ?\((.+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static MatchTeamNames Parse(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				Match regexMatch = regex.Match(text);
				if (regexMatch.Groups.Count == 3)
				{
					return new MatchTeamNames
					{
						RedTeam = regexMatch.Groups[1].Value,
						BlueTeam = regexMatch.Groups[2].Value
					};
				}
			}

			return new MatchTeamNames
			{
				RedTeam = "Red",
				BlueTeam = "Blue"
			};
		}
	}
}
