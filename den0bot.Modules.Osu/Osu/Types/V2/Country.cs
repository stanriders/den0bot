// den0bot (c) StanR 2020 - MIT License
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types.V2
{
	public class Country
	{
		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
