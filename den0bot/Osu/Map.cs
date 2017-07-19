
using System;

namespace den0bot.Osu
{
    public class Map
    {
        public uint BeatmapID;
        public uint BeatmapSetID;
        public int Status;

        public DateTime UpdatedDate;
        public DateTime RankedDate;

        public string Artist;
        public string Title;
        public string Difficulty;
        public string Creator;

        public uint MaxCombo;
        public uint DrainLength;
        public uint TotalLength;

    }
}
