// den0bot (c) StanR 2020 - MIT License

using den0bot.Util;
using System;

namespace den0bot.Modules.Osu.Osu.Types
{
	public abstract class IScore
	{
		public abstract DateTime Date { get; set; }
		public abstract ScoreGrade Grade { get; set; }
		public abstract uint Count300 { get; set; }
		public abstract uint Count100 { get; set; }
		public abstract uint Count50 { get; set; }
		public abstract uint Combo { get; set; }
		public abstract double Accuracy { get; set; }
		public abstract uint Misses { get; set; }
		public abstract LegacyMods? LegacyMods { get; set; }
		public abstract double? Pp { get; set; }

		public abstract IBeatmap Beatmap { get; set; }

		public uint ComboBasedMissCount(uint maxCombo, uint countSliders)
		{
			// guess the number of misses + slider breaks from combo
			double comboBasedMissCount;

			if (countSliders == 0)
			{
				if (Combo < maxCombo)
					comboBasedMissCount = (double)maxCombo / Combo;
				else
					comboBasedMissCount = 0;
			}
			else
			{
				double fullComboThreshold = maxCombo - 0.1 * countSliders;
				if (Combo < fullComboThreshold)
					comboBasedMissCount = fullComboThreshold / Combo;
				else
					comboBasedMissCount = Math.Pow((maxCombo - Combo) / (0.1 * countSliders), 3);
			}

			return (uint)Math.Max(Misses, Math.Floor(comboBasedMissCount));
		}
	}
}
