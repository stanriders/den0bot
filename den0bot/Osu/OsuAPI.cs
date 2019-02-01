// den0bot (c) StanR 2019 - MIT License
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using den0bot.Util;

namespace den0bot.Osu
{
	public static class OsuAPI
	{
		private static async Task<string> MakeApiRequestAsync(string request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken))
			{
				Log.Error("OsuAPI", "API Key is not defined!");
				return null;
			}
			else
				return await new WebClient().DownloadStringTaskAsync($"https://osu.ppy.sh/api/{request}&k={Config.Params.osuToken}");
		}

		/// <summary>
		/// Returns List with player's topscores from API
		/// </summary>
		public static async Task<List<Score>> GetTopscoresAsync(uint user, int amount = 5)
		{
			if (user > 0 && amount > 0)
			{
				try
				{
					return JsonConvert.DeserializeObject<List<Score>>(await MakeApiRequestAsync($"get_user_best?limit={amount}&u={user}"));
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetTopscoresAsync - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}

		/// <summary>
		/// Returns List of beatmaps from a beatmapset
		/// </summary>
		public static async Task<List<Map>> GetBeatmapSetAsync(uint beatmapsetID, bool onlyStd = true)
		{
			if (beatmapsetID > 0)
			{
				try
				{
					List<Map> set = JsonConvert.DeserializeObject<List<Map>>(await MakeApiRequestAsync($"get_beatmaps?s={beatmapsetID}"));
					if (onlyStd)
						set.RemoveAll(x => x.Mode != 0);

					set = set.OrderBy(x => x.StarRating).ToList();
					return set;
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapSetAsync - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}

		/// <summary>
		/// Returns Beatmap's info from API
		/// </summary>
		public static async Task<Map> GetBeatmapAsync(uint beatmapID)
		{
			if (beatmapID > 0)
			{
				try
				{
					return JsonConvert.DeserializeObject<List<Map>>(await MakeApiRequestAsync($"get_beatmaps?limit=1&b={beatmapID}"))[0];
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetBeatmapAsync - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}

		/// <summary>
		/// Returns player's info from API
		/// </summary>
		public static async Task<Player> GetPlayerAsync(string profileID)
		{
			if (!string.IsNullOrEmpty(profileID))
			{
				try
				{
					return JsonConvert.DeserializeObject<List<Player>>(await MakeApiRequestAsync($"get_user?u={profileID}"))[0];
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetPlayer - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}

		public static async Task<List<Score>> GetRecentScoresAsync(string user, int amount = 5)
		{
			if (!string.IsNullOrEmpty(user) && amount > 0)
			{
				try
				{
					return JsonConvert.DeserializeObject<List<Score>>(await MakeApiRequestAsync($"get_user_recent?limit={amount}&u={user}"));
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetRecentScoresAsync - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}

		public static async Task<MultiplayerMatch> GetMatch(ulong mpId)
		{
			if (mpId > 0)
			{
				try
				{
					return JsonConvert.DeserializeObject<MultiplayerMatch>(await MakeApiRequestAsync($"get_match?mp={mpId}"));
				}
				catch (Exception ex) { Log.Error("osuAPI", "GetMatch - " + ex.InnerMessageIfAny()); }
			}
			return null;
		}
	}
}
