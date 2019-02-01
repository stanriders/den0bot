// den0bot (c) StanR 2019 - MIT License
using System.IO;
using Newtonsoft.Json;

namespace den0bot.Util
{
	public static class Config
	{
		public class ConfigFile
		{
			// https://telegram.me/botfather
			public string TelegamToken { get; set; }
			public string OwnerUsername { get; set; }

			// https://osu.ppy.sh/p/api
			public string osuToken { get; set; }

			// https://osu.ppy.sh/p/irc
			public string osuIRCUsername { get; set; }
			public string osuIRCPassword { get; set; }

			public string osuLobbyName { get; set; }
			public string osuLobbyPassword { get; set; }

			// https://console.developers.google.com/apis/credentials
			public string GoogleAPIToken { get; set; }

			// http://thecatapi.com/api-key-registration.html
			public string CatToken { get; set; }
		}

		private const string config_file = "./config.json";
		public static ConfigFile Params { get; private set; } = new ConfigFile();

		public static void Init()
		{
			if (File.Exists(config_file))
				Params = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(config_file));

			// always write config back to file to update it after schema changes
			File.WriteAllText(config_file, JsonConvert.SerializeObject(Params, Formatting.Indented));
		}
	}
}
