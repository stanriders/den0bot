using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace den0bot.Osu
{
    public static class OsuAPI
    {
        /// <summary>
        /// Returns List with player's topscores from API
        /// </summary>
        public static List<Score> GetTopscores(uint user, int amount = 5)
        {
            if (user <= 0 || amount <= 0)
                return null;

            try
            {
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_user_best?k=" + Config.osu_token + "&limit=" + amount + "&u=" + user;
                    var data = web.DownloadData(request);

                    JArray arr = JArray.Parse(Encoding.UTF8.GetString(data));

                    List<Score> result = new List<Score>();
                    foreach (JToken info in arr)
                    {
                        Score topscore = new Score();
                        topscore.BeatmapID = info["beatmap_id"].Value<uint>();
                        topscore.ScoreID = info["score"].Value<uint>();
                        //topscore.Username = info["username"].ToString();
                        topscore.UserID = info["user_id"].Value<uint>();

                        topscore.Date = info["date"].Value<DateTime>();

                        topscore.Combo = info["maxcombo"].Value<uint>();
                        topscore.Perfect = Convert.ToBoolean(info["perfect"].Value<short>());

                        topscore.Count300 = info["count300"].Value<uint>();
                        topscore.Count100 = info["count100"].Value<uint>();
                        topscore.Count50 = info["count50"].Value<uint>();
                        topscore.Misses = info["countmiss"].Value<uint>();
                        topscore.CountKatu = info["countkatu"].Value<uint>();
                        topscore.CountGeki = info["countgeki"].Value<uint>();

                        topscore.EnabledMods = (Mods)Enum.Parse(typeof(Mods), info["enabled_mods"].ToString());
                        topscore.Rank = info["rank"].ToString();
                        topscore.Pp = info["pp"].Value<double>();

                        result.Add(topscore);
                    }
                    return result;
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetTopscores - " + ex.Message); }
            return null;
        }

        /// <summary>
        /// Returns Beatmap's ID from BeatmapSet
        /// </summary>
        public static uint GetBeatmapIDFromSet(uint beatmapsetID)
        {
            if (beatmapsetID <= 0)
                return 0;

            try
            {
                byte[] data;
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_beatmaps?k=" + Config.osu_token + "&s=" + beatmapsetID;
                    data = web.DownloadData(request);
                    web.Dispose();
                }

                List<JToken> arr = JArray.Parse(Encoding.UTF8.GetString(data)).OrderBy(x => x["difficultyrating"].Value<double>()).ToList();

                if (arr.Count > 0)
                {
                    JToken info = arr.Last(x => x["mode"].Value<int>() == 0); // ignore everything that is not std
                    if (info != null)
                        return info["beatmap_id"].Value<uint>();
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapIDFromSet - " + ex.Message); }

            return 0;
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
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_beatmaps?k=" + Config.osu_token + "&limit=1&b=" + beatmapID;
                    var data = web.DownloadData(request);

                    JArray arr = JArray.Parse(Encoding.UTF8.GetString(data));
                    if (arr.Count > 0)
                    {
                        JToken info = arr[0];

                        Map result = new Map();
                        result.BeatmapID = info["beatmap_id"].Value<uint>();
                        result.BeatmapSetID = info["beatmapset_id"].Value<uint>();
                        result.Status = info["approved"].Value<int>();

                        result.UpdatedDate = info["last_update"].Value<DateTime>();
                        result.RankedDate = info["approved_date"].Value<DateTime>();

                        result.Artist = info["artist"].ToString();
                        result.Title = info["title"].ToString();
                        result.Difficulty = info["version"].ToString();
                        result.Creator = info["creator"].ToString();

                        result.MaxCombo = info["max_combo"].Value<uint>();
                        result.DrainLength = info["hit_length"].Value<uint>();
                        result.TotalLength = info["total_length"].Value<uint>();

                        return result;
                    }
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetBeatmap - " + ex.Message); }

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
                using (WebClient web = new WebClient())
                {
                    string request = "https://osu.ppy.sh/api/get_user?k=" + Config.osu_token + "&u=" + profileID;
                    var data = web.DownloadData(request);

                    JArray arr = JArray.Parse(Encoding.UTF8.GetString(data));
                    if (arr.Count > 0)
                    {
                        JToken info = arr[0];

                        Player result = new Player();
                        result.ID = info["user_id"].Value<uint>();
                        result.Username = info["username"].ToString();
                        result.Country = info["country"].ToString();

                        result.Playcount = info["playcount"].Value<uint>();
                        result.Pp = info["pp_raw"].Value<double>();
                        result.Rank = info["pp_rank"].Value<uint>();
                        result.CountryRank = info["pp_country_rank"].Value<uint>();
                        result.Accuracy = info["accuracy"].Value<double>();
                        result.Level = info["level"].Value<double>();

                        result.RankedScore = info["ranked_score"].Value<ulong>();
                        result.TotalScore = info["total_score"].Value<ulong>();

                        result.Count300 = info["count300"].Value<uint>();
                        result.Count100 = info["count100"].Value<uint>();
                        result.Count50 = info["count50"].Value<uint>();
                        result.CountSS = info["count_rank_ss"].Value<uint>();
                        result.CountS = info["count_rank_s"].Value<uint>();
                        result.CountA = info["count_rank_a"].Value<uint>();

                        return result;
                    }
                }
            }
            catch (Exception ex) { Log.Error("osuAPI", "GetPlayer - " + ex.Message); }

            return null;
        }
    }
}
