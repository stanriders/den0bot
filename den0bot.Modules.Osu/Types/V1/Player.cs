// den0bot (c) StanR 2020 - MIT License

using System;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V1
{
	public class Player
	{
		[JsonProperty("user_id")]
		public uint ID { get; set; }
		[JsonProperty("username")]
		public string Username { get; set; }
		[JsonProperty("country")]
		public string Country { get; set; }
		[JsonProperty("join_date")]
		public DateTime JoinDate { get; set; }
		
		[JsonProperty("playcount")]
		public uint Playcount { get; set; }
		[JsonProperty("total_seconds_played")]
		public uint PlaytimeSeconds { get; set; }

		[JsonProperty("pp_raw")]
		public double Pp { get; set; }
		[JsonProperty("pp_rank")]
		public uint Rank { get; set; }
		[JsonProperty("pp_country_rank")]
		public uint CountryRank { get; set; }
		[JsonProperty("accuracy")]
		public double Accuracy { get; set; }

		[JsonProperty("level")]
		public double Level { get; set; }
		[JsonProperty("ranked_score")]
		public ulong RankedScore { get; set; }
		[JsonProperty("total_score")]
		public ulong TotalScore { get; set; }

		[JsonProperty("count300")]
		public uint Count300 { get; set; }
		[JsonProperty("count100")]
		public uint Count100 { get; set; }
		[JsonProperty("count50")]
		public uint Count50 { get; set; }
		[JsonProperty("count_rank_ss")]
		public uint CountSS { get; set; }
		[JsonProperty("count_rank_s")]
		public uint CountS { get; set; }
		[JsonProperty("count_rank_a")]
		public uint CountA { get; set; }
	}
}
