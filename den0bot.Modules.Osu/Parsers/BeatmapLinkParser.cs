// den0bot (c) StanR 2024 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using den0bot.Modules.Osu.Types.Enums;
using den0bot.Util;
using osu.Game.Rulesets.Mods;
using Match = System.Text.RegularExpressions.Match;

namespace den0bot.Modules.Osu.Parsers
{
	public class BeatmapLinkData
	{
		public int ID { get; set; }
		public bool IsBeatmapset { get; set; }
		public Mode Mode { get; set; }
		public Mod[] Mods { get; set; } = null!;
	}

	public static class BeatmapLinkParser
	{
		private static readonly Regex linkRegex = new(@"(?>https?:\/\/)?(?>osu|old)\.ppy\.sh\/([b,s]|(?>beatmaps)|(?>beatmapsets))\/(\d+)\/?\#?(\w+)?\/?(\d+)?\/?(?>[&,?].+=\w+)?\s?(?>\+(\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static BeatmapLinkData? Parse(string text)
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
							ID = int.Parse(regexGroups[2].Value),
							IsBeatmapset = true,
							Mods = mods
						};
					}
					else
					{
						return new BeatmapLinkData
						{
							// 'beatmaps' case is stupid and since it's literally one of a kind we're accounting for it here
							ID = regexGroups[1].Value == "beatmaps" ? int.Parse(regexGroups[2].Value) : int.Parse(regexGroups[4].Value),
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
						ID = int.Parse(regexGroups[2].Value),
						IsBeatmapset = isSet,
						Mods = mods
					};
				}
			}

			return null;
		}

		private static Mod[] ConvertToMods(string modString)
		{
			var mods = new List<Mod>();
			for (var i = 0; i < modString.Length; i += 2)
			{
				var mod = PpCalculation.Osu.AllMods
					.Where(x => x.Acronym == modString.Substring(i, 2))
					.Select(x=> x.CreateInstance())
					.FirstOrDefault();

				if (mod != null)
				{
					mods.Add(mod);
				}
			}
			
			return mods.ToArray();
		}
	}
}
