// den0bot (c) StanR 2021 - MIT License
using System;

namespace den0bot.Modules.Osu.Types.Enums
{
	/// <summary>
	/// Bitwise list of all mods
	/// </summary>
	[Flags]
	public enum LegacyMods
	{
		NM = 0,
		NF = 1,
		EZ = 2,
		TD = 4, // previously NoVideo, now TouchDevice
		HD = 8,
		HR = 16,
		SD = 32,
		DT = 64,
		HT = 256,
		NC = 512, // Only set along with DoubleTime. i.e: NC only gives 576
		FL = 1024,
		SO = 4096,
		PF = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
		FI = 1048576, // mania
		Random = 2097152, // mania
		V2 = 536870912,

		DifficultyChanging = NC | HT | DT | HR | EZ, // FL changes difficulty but osu! api doesn't think so
	}
}
