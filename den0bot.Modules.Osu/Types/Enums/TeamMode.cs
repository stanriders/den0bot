// den0bot (c) StanR 2021 - MIT License

using System.ComponentModel;

namespace den0bot.Modules.Osu.Types.Enums
{
	public enum TeamMode
	{
		[Description(@"head-to-head")]
		HeadToHead,

		[Description(@"tag")]
		Tag,

		[Description(@"team-vs")]
		Team,

		[Description(@"team-tag")]
		TeamTag
	}
}
