// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace den0bot.Osu
{
    public static class OsuAPI
    {
        public static long RequestCount = 0;

        private static string MakeApiRequest(string request)
        {
            if (RequestCount < long.MaxValue)
                RequestCount++;

            return new WebClient().DownloadString("https://osu.ppy.sh/api/" + request);
        }
        private static async Task<string> MakeApiRequestAsync(string request)
        {
            if (RequestCount < long.MaxValue)
                RequestCount++;

            return await new WebClient().DownloadStringTaskAsync("https://osu.ppy.sh/api/" + request);
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

        public static async Task<List<Score>> GetTopscoresAsync(uint user, int amount = 5)
        {
            if (user <= 0 || amount <= 0)
                return null;

            try
            {
                string request = "get_user_best?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;
                return JsonConvert.DeserializeObject<List<Score>>(await MakeApiRequestAsync(request));
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetTopscoresAsync - " + ex.InnerMessageIfAny()); }
            return null;
        }

        /// <summary>
        /// Returns List of beatmaps from a beatmapset
        /// </summary>
        public static async Task<List<Map>> GetBeatmapSetAsync(uint beatmapsetID)
        {
            if (beatmapsetID <= 0)
                return null;

            try
            {
                string request = "get_beatmaps?k=" + Config.osu_token + "&s=" + beatmapsetID;

                List<Map> set = JsonConvert.DeserializeObject<List<Map>>(await MakeApiRequestAsync(request));
                set.RemoveAll(x => x.Mode != 0);
                set = set.OrderBy(x => x.StarRating).ToList();
                return set;
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapSetAsync - " + ex.InnerMessageIfAny()); }

            return null;
        }

        /// <summary>
        /// Returns Beatmap's info from API
        /// </summary>
        public static async Task<Map> GetBeatmapAsync(uint beatmapID)
        {
            if (beatmapID <= 0)
                return null;

            try
            {
                string request = "get_beatmaps?k=" + Config.osu_token + "&limit=1&b=" + beatmapID;
                return JsonConvert.DeserializeObject<List<Map>>(await MakeApiRequestAsync(request))[0];
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapAsync - " + ex.InnerMessageIfAny()); }

            return null;
        }

        /// <summary>
        /// Returns player's info from API
        /// </summary>
        public static async Task<Player> GetPlayerAsync(string profileID)
        {
            if (profileID == string.Empty)
                return null;

            try
            {
                string request = "get_user?k=" + Config.osu_token + "&u=" + profileID;
                return JsonConvert.DeserializeObject<List<Player>>(await MakeApiRequestAsync(request))[0];
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetPlayer - " + ex.InnerMessageIfAny()); }

            return null;
        }

        public static async Task<List<Score>> GetRecentScoresAsync(string user, int amount = 5)
        {
            if (string.IsNullOrEmpty(user) || amount <= 0)
                return null;

            try
            {
                string request = "get_user_recent?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;
                return JsonConvert.DeserializeObject<List<Score>>(await MakeApiRequestAsync(request));
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetRecentScoresAsync - " + ex.InnerMessageIfAny()); }
            return null;
        }
    }
}
