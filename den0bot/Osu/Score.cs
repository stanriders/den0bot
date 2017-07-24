﻿using System;

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

        public override bool Equals(object obj)
        {
            Score b = obj as Score;
            if (ReferenceEquals(this, b))
                return true;

            if (b == null)
                return false;

            return ScoreID == b.ScoreID && Date == b.Date;
        }
        public override int GetHashCode()
        {
            return ScoreID.GetHashCode() ^ Date.GetHashCode();
        }
        public static bool operator ==(Score a, Score b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(Score a, Score b)
        {
            return !Equals(a, b);
        }
    }
}
