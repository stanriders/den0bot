﻿// den0bot (c) StanR 2024 - MIT License
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;

namespace den0bot.Util
{
	public class ConfigFile
	{
		public List<string> Modules { get; } = new();

		public bool UseEvents { get; set; } = false;

		// https://telegram.me/botfather
		[Required]
		public string TelegamToken { get; set; }

		[Required]
		public string OwnerUsername { get; set; }

		// http://thecatapi.com/api-key-registration.html
		public string? CatToken { get; set; }

		public string? SentryDsn { get; set; }

		public string? WeatherToken { get; set; }
	}

	internal static class Config
	{
		private const string config_file = "./data/config.json";
		public static ConfigFile Params { get; } = new();

		static Config()
		{
			if (File.Exists(config_file))
				Params = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(config_file));

			// always write config back to file to update it after schema changes
			File.WriteAllText(config_file, JsonConvert.SerializeObject(Params, Formatting.Indented));
		}
	}
}
