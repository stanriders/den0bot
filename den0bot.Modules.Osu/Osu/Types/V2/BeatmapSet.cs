// den0bot (c) StanR 2020 - MIT License
using System.Collections.Generic;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Osu.Types.V2
{
	public class BeatmapSetShort
	{
		[JsonProperty("id")]
		public uint Id { get; set; }

		[JsonProperty("artist")]
		public string Artist { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("creator")]
		public string CreatorName { get; set; }

		[JsonProperty("user_id")]
		public uint CreatorId { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("preview_url")]
		public string PreviewUrl { get; set; }

		[JsonProperty("covers")]
		public BeatmapSetCovers Covers { get; set; }

		public class BeatmapSetCovers
		{
			[JsonProperty("cover")]
			public string Cover { get; set; }
			[JsonProperty("cover@2x")]
			public string Cover2X { get; set; }

			[JsonProperty("card")]
			public string Card { get; set; }
			[JsonProperty("card@2x")]
			public string Card2X { get; set; }

			[JsonProperty("list")]
			public string List { get; set; }
			[JsonProperty("list@2x")]
			public string List2X { get; set; }

			[JsonProperty("slimcover")]
			public string Slimcover { get; set; }
			[JsonProperty("slimcover@2x")]
			public string Slimcover2X { get; set; }
		}

		/*
            "favourite_count": 518,
            "play_count": 1629819,
            "source": "Touhou",
            "user_id": 1612624,
            "video": false
			*/
	}

	public class BeatmapSet : BeatmapSetShort
	{
		[JsonProperty("beatmaps")]
		public List<Beatmap> Beatmaps { get; set; }

		[JsonProperty("user")]
		public UserShort Creator { get; set; }
	}
}
