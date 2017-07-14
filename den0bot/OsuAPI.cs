using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace den0bot
{
    public static class OsuAPI
    {
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
