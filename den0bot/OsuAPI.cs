using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace den0bot
{
    public static class OsuAPI
    {
        /// <summary>
        /// Bitwise list of all mods
        /// </summary>
        [Flags]
        public enum Mods
        {
            None = 0,
            NF = 1,
            EZ = 2,
            HD = 8,
            HR = 16,
            SD = 32,
            DT = 64,
            HT = 256,
            NC = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            FL = 1024,
            SO = 4096,
            PF = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
            Key4 = 32768,
            Key5 = 65536,
            Key6 = 131072,
            Key7 = 262144,
            Key8 = 524288,
            FadeIn = 1048576,
            Random = 2097152,
            LastMod = 4194304,
            Key9 = 16777216,
            Key10 = 33554432,
            Key1 = 67108864,
            Key3 = 134217728,
            Key2 = 268435456
        }

        /// <summary>
        /// Returns JArray with player's topscores from API
        /// </summary>
        public static JArray GetLastTopscores(int user, int amount = 5)
        {
            JArray result = null;

            if (user <= 0 || amount <= 0)
                return result;

            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_user_best?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;

                    var data = web.DownloadData(request);

                    result = JArray.Parse(Encoding.UTF8.GetString(data));
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetLastTopscore - " + ex.Message); }

            return result;
        }

        /// <summary>
        /// Returns JToken with beatmap's info from API
        /// </summary>
        public static JToken GetBeatmapInfo(uint beatmapID)
        {
            JToken result = null;

            if (beatmapID <= 0)
                return result;

            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_beatmaps?k=" + Config.osu_token + "&limit=1&b=" + beatmapID;

                    var data = web.DownloadData(request);

                    result = JArray.Parse(Encoding.UTF8.GetString(data))[0];
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapInfo - " + ex.Message); }

            return result;
        }

        /// <summary>
        /// Returns JToken with player's info from API
        /// </summary>
        public static JToken GetPlayerInfo(string profileID)
        {
            JToken result = null;

            if (profileID == string.Empty)
                return result;

            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_user?k=" + Config.osu_token + "&u=" + profileID;

                    var data = web.DownloadData(request);

                    result = JArray.Parse(Encoding.UTF8.GetString(data))[0];
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetPlayerInfo - " + ex.Message); }

            return result;
        }
    }
}
