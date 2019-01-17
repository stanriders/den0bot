// den0bot (c) StanR 2019 - MIT License
using System;
using SQLite;

namespace den0bot.DB.Types
{
	class Misc
	{
		[PrimaryKey]
		public bool Hi { get; set; }

		public int CurrentMPLobby { get; set; }

		public int GirlSeason { get; set; }

		public DateTime GirlSeasonStart { get; set; }
	}
}