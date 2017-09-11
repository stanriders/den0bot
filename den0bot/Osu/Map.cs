// den0bot (c) StanR 2017 - MIT License
using System;

namespace den0bot.Osu
{
    public class Map
    {
        public uint BeatmapID;
        public uint BeatmapSetID;

        public DateTime UpdatedDate;
        public DateTime RankedDate;
        public RankedStatus Status;

        public string Artist;
        public string Title;
        public string Difficulty;
        public string Creator;

        public double StarRating;
        public double CS;
        public double AR;
        public double OD;
        public double HP;

        public uint MaxCombo;
        public uint DrainLength;
        public uint TotalLength;

        public double BPM;

        public string Thumbnail()
        {
            return "https://assets.ppy.sh/beatmaps/"+BeatmapSetID+"/covers/cover.jpg";
        }
    }
}
