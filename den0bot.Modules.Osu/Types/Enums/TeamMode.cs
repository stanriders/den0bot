// den0bot (c) StanR 2021 - MIT License
using System.Runtime.Serialization;

namespace den0bot.Modules.Osu.Types.Enums
{
	public enum TeamMode
	{
		[EnumMember(Value = @"head-to-head")]
		HeadToHead,

		[EnumMember(Value = "tag")]
		Tag,

		[EnumMember(Value = @"team-vs")]
		Team,

		[EnumMember(Value = @"team-tag")]
		TeamTag
	}
}
