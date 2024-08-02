// den0bot (c) StanR 2024 - MIT License
using System.Collections.Generic;
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
	public class BeatmapSetShort
	{
		[JsonProperty("id")]
		public uint Id { get; set; }

		[JsonProperty("artist")]
		public string Artist { get; set; } = null!;

		[JsonProperty("title")]
		public string Title { get; set; } = null!;

		[JsonProperty("creator")]
		public string CreatorName { get; set; } = null!;

		[JsonProperty("user_id")]
		public uint CreatorId { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; } = null!;

		[JsonProperty("preview_url")]
		public string PreviewUrl { get; set; } = null!;

		[JsonProperty("covers")]
		public BeatmapSetCovers Covers { get; set; } = null!;

		public class BeatmapSetCovers
		{
			[JsonProperty("cover")]
			public string Cover { get; set; } = null!;
			[JsonProperty("cover@2x")]
			public string Cover2X { get; set; } = null!;

			[JsonProperty("card")]
			public string Card { get; set; } = null!;
			[JsonProperty("card@2x")]
			public string Card2X { get; set; } = null!;

			[JsonProperty("list")]
			public string List { get; set; } = null!;
			[JsonProperty("list@2x")]
			public string List2X { get; set; } = null!;

			[JsonProperty("slimcover")]
			public string Slimcover { get; set; } = null!;
			[JsonProperty("slimcover@2x")]
			public string Slimcover2X { get; set; } = null!;
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
		public List<Beatmap> Beatmaps { get; set; } = null!;

		[JsonProperty("converts")]
		public List<Beatmap> Converts { get; set; } = null!;

		[JsonProperty("user")]
		public UserShort Creator { get; set; } = null!;
	}
}
