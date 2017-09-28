// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace den0bot.Osu
{
    public static class OsuAPI
    {
        private static string MakeApiRequest(string request)
        {
            WebClient web = new WebClient();
            string data = web.DownloadString("https://osu.ppy.sh/api/" + request);
            web.Dispose();
            return data;
        }

        /// <summary>
        /// Returns List with player's topscores from API
        /// </summary>
        public static List<Score> GetTopscores(uint user, int amount = 5)
        {
            if (user <= 0 || amount <= 0)
                return null;

            try
            {
                string request = "get_user_best?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;
                return JsonConvert.DeserializeObject<List<Score>>(MakeApiRequest(request));
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetTopscores - " + ex.InnerMessageIfAny()); }
            return null;
        }

        /// <summary>
        /// Returns List of beatmaps from a beatmapset
        /// </summary>
        public static List<Map> GetBeatmapSet(uint beatmapsetID)
        {
            if (beatmapsetID <= 0)
                return null;

            try
            {
                string request = "get_beatmaps?k=" + Config.osu_token + "&s=" + beatmapsetID;

                List<Map> set = JsonConvert.DeserializeObject<List<Map>>(MakeApiRequest(request));
                set.OrderBy(x => x.StarRating);
                set.RemoveAll(x => x.Mode != 0);
                return set;
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapSet - " + ex.InnerMessageIfAny()); }

            return null;
        }

        /// <summary>
        /// Returns Beatmap's info from API
        /// </summary>
        public static Map GetBeatmap(uint beatmapID)
        {
            if (beatmapID <= 0)
                return null;

            try
            {
                string request = "get_beatmaps?k=" + Config.osu_token + "&limit=1&b=" + beatmapID;
                return JsonConvert.DeserializeObject<List<Map>>(MakeApiRequest(request))[0];
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmap - " + ex.InnerMessageIfAny()); }

            return null;
        }

        /// <summary>
        /// Returns player's info from API
        /// </summary>
        public static Player GetPlayer(string profileID)
        {
            if (profileID == string.Empty)
                return null;

            try
            {
                string request = "get_user?k=" + Config.osu_token + "&u=" + profileID;
                return JsonConvert.DeserializeObject<List<Player>>(MakeApiRequest(request))[0];
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetPlayer - " + ex.InnerMessageIfAny()); }

            return null;
        }

        public static List<Score> GetRecentScores(string user, int amount = 5)
        {
            if (string.IsNullOrEmpty(user) || amount <= 0)
                return null;

            try
            {
                string request = "get_user_recent?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;
                return JsonConvert.DeserializeObject<List<Score>>(MakeApiRequest(request));
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetRecentScores - " + ex.InnerMessageIfAny()); }
            return null;
        }
    }
}
