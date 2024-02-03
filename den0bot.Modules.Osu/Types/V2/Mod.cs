using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace den0bot.Modules.Osu.Types.V2
{
	public class Mod
	{
		public string Acronym { get; set; }

		public Dictionary<string, string> Settings { get; set; }
	}
}
