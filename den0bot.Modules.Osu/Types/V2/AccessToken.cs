// den0bot (c) StanR 2021 - MIT License
using Newtonsoft.Json;
using System;

namespace den0bot.Modules.Osu.Types.V2
{
	public class AccessToken
	{
		[JsonProperty("token_type")]
		public string Type { get; set; }

		public DateTime expireDate;

		[JsonProperty("expires_in")]
		public int ExpiresIn
		{
			set => expireDate = DateTime.Now.AddSeconds(value);
		}

		public bool Expired => expireDate < DateTime.Now;

		[JsonProperty("access_token")]
		public string Token { get; set; }
	}
}
