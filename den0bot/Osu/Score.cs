// den0bot (c) StanR 2017 - MIT License
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

        public double Accuracy()
        {
            /*
             * Accuracy = Total points of hits / (Total number of hits * 300)
             * Total points of hits  =  Number of 50s * 50 + Number of 100s * 100 + Number of 300s * 300
             * Total number of hits  =  Number of misses + Number of 50's + Number of 100's + Number of 300's
             */

            double totalPoints = Count50 * 50 + Count100 * 100 + Count300 * 300;
            double totalHits = Misses + Count50 + Count100 + Count300;

            return totalPoints / (totalHits * 300) * 100;

        }

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
