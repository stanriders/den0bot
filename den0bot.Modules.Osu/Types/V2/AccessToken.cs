// den0bot (c) StanR 2024 - MIT License
using Newtonsoft.Json;
using System;

namespace den0bot.Modules.Osu.Types.V2
{
	public class AccessToken
	{
		[JsonProperty("token_type")]
		public string Type { get; set; } = null!;

		private DateTime expireDate;
		[JsonProperty("expires_in")]
		public long ExpiresIn
		{
			get => expireDate.Ticks;
			set => expireDate = DateTime.Now.AddSeconds(value);
		}

		public bool Expired => expireDate < DateTime.Now;

		[JsonProperty("access_token")]
		public string Token { get; set; } = null!;
	}
}
