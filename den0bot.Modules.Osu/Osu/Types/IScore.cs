// den0bot (c) StanR 2020 - MIT License

using System;

namespace den0bot.Modules.Osu.Osu.Types
{
	public abstract class IScore
	{
		public abstract DateTime Date { get; set; }
		public abstract string Grade { get; set; }
		public abstract uint Count300 { get; set; }
		public abstract uint Count100 { get; set; }
		public abstract uint Count50 { get; set; }
		public abstract uint Combo { get; set; }
		public abstract double Accuracy { get; set; }
		public abstract uint Misses { get; set; }
		public abstract LegacyMods? LegacyMods { get; set; }
		public abstract double? Pp { get; set; }

		public abstract IBeatmap Beatmap { get; set; }
	}
}
