// den0bot (c) StanR 2017 - MIT License
using System;
using Newtonsoft.Json;

namespace den0bot.Osu
{
    public class Map
    {
        [JsonProperty("beatmap_id")]
        public uint BeatmapID;
        [JsonProperty("beatmapset_id")]
        public uint BeatmapSetID;

        [JsonProperty("last_update")]
        public DateTime UpdatedDate;
        [JsonProperty("approved_date")]
        public DateTime RankedDate;
        [JsonProperty("approved")]
        public RankedStatus Status;

        [JsonProperty("artist")]
        public string Artist;
        [JsonProperty("title")]
        public string Title;
        [JsonProperty("version")]
        public string Difficulty;
        [JsonProperty("creator")]
        public string Creator;
        [JsonProperty("mode")]
        public Mode Mode;

        [JsonProperty("difficultyrating")]
        public double StarRating;
        [JsonProperty("diff_size")]
        public double CS;
        [JsonProperty("diff_approach")]
        public double AR;
        [JsonProperty("diff_overall")]
        public double OD;
        [JsonProperty("diff_drain")]
        public double HP;

        [JsonProperty("max_combo")]
        public uint MaxCombo;
        [JsonProperty("hit_length")]
        public uint DrainLength;
        [JsonProperty("total_length")]
        public uint TotalLength;

        [JsonProperty("bpm")]
        public double BPM;

        public string Thumbnail
        {
            get { return "https://assets.ppy.sh/beatmaps/" + BeatmapSetID + "/covers/cover.jpg"; }
        }
    }
}
