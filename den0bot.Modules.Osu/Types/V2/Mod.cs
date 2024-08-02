// den0bot (c) StanR 2024 - MIT License
using System.Collections.Generic;

namespace den0bot.Modules.Osu.Types.V2
{
	public class Mod
	{
		public string Acronym { get; set; } = null!;

		public Dictionary<string, string>? Settings { get; set; }
	}
}
