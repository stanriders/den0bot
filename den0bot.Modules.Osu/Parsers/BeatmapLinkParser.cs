// den0bot (c) StanR 2021 - MIT License
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Util;

namespace den0bot.Modules.Osu.Parsers
{
	public class BeatmapLinkData
	{
		public uint ID { get; set; }
		public bool IsBeatmapset { get; set; }
		public Mode Mode { get; set; }
		public LegacyMods Mods { get; set; }
	}

	public static class BeatmapLinkParser
	{
		private static readonly Regex linkRegex = new(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/([b,s]|(?>beatmaps)|(?>beatmapsets))\/(\d+)\/?\#?(\w+)?\/?(\d+)?\/?(?>[&,?].+=\w+)?\s?(?>\+(\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static BeatmapLinkData Parse(string text)
		{
			if (string.IsNullOrEmpty(text))
				return null;

			Match regexMatch = linkRegex.Match(text);
			if (regexMatch.Groups.Count > 1)
			{
				var regexGroups = regexMatch.Groups.Values.ToArray();
				// Groups:
				// 0 - full match
				// 1 - link type (b/s/beatmapsets/beatmaps)
				// 2 - beatmapset id (beatmap id if link type is 'beatmaps/b/s')
				// 3 - game mode
				// 4 - beatmap id
				// 5 - mods

				bool isNew = regexGroups[1].Value != "b" && regexGroups[1].Value != "s"; // are we using new website or not
				bool isSet = (regexGroups[1].Value == "beatmapsets" && regexGroups.Count(x => x.Success) < 4) || regexGroups[1].Value == "s";

				var mods = ConvertToMods(regexGroups[5].Value);
				var mode = Mode.Osu;
				if (!string.IsNullOrEmpty(regexGroups[3].Value))
					Enum.TryParse(regexGroups[3].Value.Capitalize(), out mode);

				if (isNew)
				{
					if (isSet)
					{
						return new BeatmapLinkData
						{
							ID = uint.Parse(regexGroups[2].Value),
							IsBeatmapset = true,
							Mods = mods
						};
					}
					else
					{
						return new BeatmapLinkData
						{
							// 'beatmaps' case is stupid and since it's literally one of a kind we're accounting for it here
							ID = regexGroups[1].Value == "beatmaps" ? uint.Parse(regexGroups[2].Value) : uint.Parse(regexGroups[4].Value),
							IsBeatmapset = false,
							Mode = mode,
							Mods = mods
						};
					}
				}
				else
				{
					return new BeatmapLinkData
					{
						ID = uint.Parse(regexGroups[2].Value),
						IsBeatmapset = isSet,
						Mods = mods
					};
				}
			}

			return null;
		}

		private static LegacyMods ConvertToMods(string mods)
		{
			if (string.IsNullOrEmpty(mods) || mods.Length > 36) // every mod combination possible
				return LegacyMods.NM;

			if (Enum.TryParse(mods, true, out LegacyMods result))
				return result;

			StringBuilder builder = new StringBuilder(mods.Length * 2);
			bool secondChar = false;
			foreach (char c in mods)
			{
				builder.Append(c);
				if (secondChar)
				{
					builder.Append(',');
					builder.Append(' ');
				}
				secondChar = !secondChar;
			}
			builder.Remove(builder.Length - 2, 2);
			Enum.TryParse(builder.ToString(), true, out result);
			return result;
		}
	}
}
