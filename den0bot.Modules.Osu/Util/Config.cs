// den0bot (c) StanR 2024 - MIT License
using System.IO;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Util
{
	internal static class Config
	{
		public class ConfigFile
		{
			// https://osu.ppy.sh/p/api
			public string osuToken { get; set; } = null!;
			public string osuClientId { get; set; } = null!;
			public string osuClientSecret { get; set; } = null!;

			// https://console.developers.google.com/apis/credentials
			public string GoogleAPIToken { get; set; } = null!;
			public string YoutubeChannelId { get; set; } = null!;
		}

		private const string config_file = "./Modules/osuconfig.json";
		public static ConfigFile Params { get; } = new();

		static Config()
		{
			if (File.Exists(config_file))
				Params = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(config_file))!;

			// always write config back to file to update it after schema changes
			File.WriteAllText(config_file, JsonConvert.SerializeObject(Params, Formatting.Indented));
		}
	}
}
