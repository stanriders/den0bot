// den0bot (c) StanR 2020 - MIT License
using System;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types.V2
{
	public class UserShort
	{
		[JsonProperty("id")]
		public uint Id { get; set; }

		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonProperty("country")]
		public Country Country { get; set; }

		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		[JsonProperty("avatar_url")]
		public string AvatarUrl { get; set; }

		[JsonProperty("is_active")]
		public bool IsActive { get; set; }

		[JsonProperty("is_bot")]
		public bool IsBot { get; set; }

		[JsonProperty("is_online")]
		public bool IsOnline { get; set; }

		[JsonProperty("is_supporter")]
		public bool IsSupporter { get; set; }

		[JsonProperty("last_visit")]
		public DateTime? LastVisit { get; set; }

		/*
		"default_group": "default",
	    "pm_friends_only": false,
	    "profile_colour": null,
		*/
	}

	public class User : UserShort
	{
		[JsonProperty("join_date")]
		public DateTime JoinDate { get; set; }

		[JsonProperty("statistics")]
		public UserStatistics Statistics { get; set; }

		[JsonProperty("cover_url")]
		public string CoverUrl { get; set; }

		[JsonProperty("previous_usernames")]
		public string[] PreviousUsernames { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("badges")]
		public TournamentBadge[] Badges { get; set; }

		[JsonProperty("follower_count")]
		public uint Followers { get; set; }

		[JsonProperty("monthly_playcounts")]
		public TimeframeCount[] MonthlyPlays { get; set; }

		[JsonProperty("replays_watched_counts")]
		public TimeframeCount[] ReplaysWatched { get; set; }

		[JsonProperty("rankHistory")]
		public RankHistory Ranks { get; set; }

		public class UserCover
		{
			[JsonProperty("custom_url")]
			public string CustomUrl { get; set; }

			[JsonProperty("url")]
			public string Url { get; set; }

			[JsonProperty("id")]
			public int? Id { get; set; }
		}

		public class TournamentBadge
		{
			[JsonProperty("awarded_at")]
			public DateTime AwardedAt { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("image_url")]
			public string ImageUrl { get; set; }

			[JsonProperty("url")]
			public string Url { get; set; }
		}

		public class RankHistory
		{
			[JsonProperty("mode")]
			public string Mode { get; set; }

			[JsonProperty("data")]
			public int[] Ranks { get; set; }
		}

		/*
	    "discord": "StanR#3012",
	    "has_supported": true,
	    "interests": null,
	    "kudosu": {
	        "total": 2,
	        "available": 1
	    },
	    "lastfm": null,
	    "location": "Saint-Petersburg",
	    "max_blocks": 50,
	    "max_friends": 500,
	    "occupation": "no pp - no shiawase",
	    "playmode": "osu",
	    "playstyle": [],
	    "post_count": 42,
	    "profile_order": [],
	    "skype": null,
	    "twitter": "stanriders",
	    "website": "http://StanR.info",
	    "account_history": [],
	    "active_tournament_banner": [],
	    "favourite_beatmapset_count": 62,
	    "graveyard_beatmapset_count": 11,
	    "groups": [],
	    "loved_beatmapset_count": 0,
	    "page": {
	        "html": "",
	        "raw": ""
	    },
	    "ranked_and_approved_beatmapset_count": 0,
	    "scores_first_count": 0,
	    "support_level": 2,
	    "unranked_beatmapset_count": 0,
	    "user_achievements": [
	        {
	            "achieved_at": "2019-08-22T17:11:15+00:00",
	            "achievement_id": 38
	        }
	    ]*/
	}
	public class UserStatistics
	{
		[JsonProperty("play_count")]
		public uint Playcount { get; set; }

		[JsonProperty("play_time")]
		public uint PlaytimeSeconds { get; set; }

		[JsonProperty("pp")]
		public double Pp { get; set; }

		[JsonProperty("rank")]
		public UserRank Rank { get; set; }

		[JsonProperty("pp_country_rank")]
		public uint CountryRank { get; set; }

		[JsonProperty("hit_accuracy")]
		public double Accuracy { get; set; }

		[JsonProperty("level")]
		public UserLevel Level { get; set; }

		[JsonProperty("ranked_score")]
		public ulong RankedScore { get; set; }

		[JsonProperty("total_score")]
		public ulong TotalScore { get; set; }

		[JsonProperty("total_hits")]
		public ulong TotalHits { get; set; }

		[JsonProperty("maximum_combo")]
		public uint MaximumCombo { get; set; }

		[JsonProperty("replays_watched_by_others")]
		public uint TeplaysWatched { get; set; }

		[JsonProperty("grade_counts")]
		public UserGradeCounts GradeCounts { get; set; }

		[JsonProperty("is_ranked")]
		public bool HasRank { get; set; }

		public class UserRank
		{
			[JsonProperty("global")]
			public uint Global { get; set; }
			[JsonProperty("country")]
			public uint Country { get; set; }
		}

		public class UserLevel
		{
			[JsonProperty("current")]
			public uint Current { get; set; }
			[JsonProperty("progress")]
			public uint Progress { get; set; }
		}

		public class UserGradeCounts
		{
			[JsonProperty("ss")]
			public uint SS { get; set; }
			[JsonProperty("s")]
			public uint S { get; set; }
			[JsonProperty("a")]
			public uint A { get; set; }
		}
	}
}
