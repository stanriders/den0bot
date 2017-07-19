using System;

namespace den0bot.Osu
{
    public class Score
    {
        public uint BeatmapID;
        public uint ScoreID;
        //public string Username;
        public uint UserID;

        public DateTime Date;

        public uint Combo;
        public bool Perfect;

        public uint Count300;
        public uint Count100;
        public uint Count50;
        public uint Misses;
        public uint CountKatu;
        public uint CountGeki;

        public Mods EnabledMods;
        public string Rank;
        public double Pp;
    }
}
