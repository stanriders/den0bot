using System.Collections.Generic;

namespace den0bot.Modules.Osu.Types.V2
{
	public class Mod
	{
		public string Acronym { get; set; }

		public Dictionary<string, string>? Settings { get; set; }
	}
}
