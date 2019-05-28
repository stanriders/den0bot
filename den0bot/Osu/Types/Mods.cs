// den0bot (c) StanR 2019 - MIT License
using System;

namespace den0bot.Osu.Types
{
	/// <summary>
	/// Bitwise list of all mods
	/// </summary>
	[Flags]
	public enum Mods
	{
		None = 0,
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
		/*Key4 = 32768,
		Key5 = 65536,
		Key6 = 131072,
		Key7 = 262144,
		Key8 = 524288,
		FadeIn = 1048576,
		Random = 2097152,
		LastMod = 4194304,
		Key9 = 16777216,
		Key10 = 33554432,
		Key1 = 67108864,
		Key3 = 134217728,
		Key2 = 268435456*/
	}
}
