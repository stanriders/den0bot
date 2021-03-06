﻿// den0bot (c) StanR 2021 - MIT License
using System;
using den0bot.Modules.Osu.Types.Enums;

namespace den0bot.Modules.Osu.Util
{
	internal static class OsuExtensions
	{
		public static string ReadableMods(this LegacyMods value)
		{
			var res = value.ToString()
				.Replace(",", string.Empty)
				.Replace(" ", string.Empty);

			if (value.HasFlag(LegacyMods.NC))
				res = res.Replace("DT", string.Empty);

			if (value.HasFlag(LegacyMods.PF))
				res = res.Replace("SD", string.Empty);

			return res;
		}

		public static string[] ToArray(this LegacyMods value, bool trimNomod = true)
		{
			if (trimNomod && value == LegacyMods.NM)
				return Array.Empty<string>();

			return value.ToString().Split(", ");
		}
	}
}
