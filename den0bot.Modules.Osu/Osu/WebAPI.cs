// den0bot (c) StanR 2020 - MIT License
using System;
using System.Threading.Tasks;
using den0bot.Util;
using den0bot.Modules.Osu.Osu.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules.Osu.Osu
{
	public static class WebApi
	{
		private static string v2AccessToken = string.Empty;
		private static DateTime v2AccessTokenExpiration = DateTime.Now;

		public static async Task<dynamic> MakeApiRequest(IRequest request)
		{
			return request.API switch
			{
				APIVersion.V1 => await V1ApiRequest(request),
				APIVersion.V2 => await V2ApiRequest(request),
				_ => throw new NotImplementedException(),
			};
		}

		private static async Task<dynamic> V1ApiRequest(IRequest request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken))
			{
				Log.Error("API Key is not defined!");
				return null;
			}

			try
			{
				string json =
					await Web.DownloadString($"https://osu.ppy.sh/api/{request.Address}&k={Config.Params.osuToken}");

				dynamic deserializedObject = JsonConvert.DeserializeObject(json, request.ReturnType, settings: new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
				if (request.ShouldReturnSingle)
					return deserializedObject[0];

				return deserializedObject;
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
				return null;
			}
		}

		private static async Task<dynamic> V2ApiRequest(IRequest request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuClientId) || string.IsNullOrEmpty(Config.Params.osuClientSecret))
			{
				Log.Error("API Key is not defined!");
				return null;
			}

			if (string.IsNullOrEmpty(v2AccessToken) || v2AccessTokenExpiration < DateTime.Now)
			{
				var authRequest = new
				{
					client_id = Config.Params.osuClientId,
					client_secret = Config.Params.osuClientSecret,
					grant_type = "client_credentials",
					scope = "public"
				};

				string authJson = await Web.PostJson("https://osu.ppy.sh/oauth/token", JsonConvert.SerializeObject(authRequest));
				if (!string.IsNullOrEmpty(authJson))
				{
					var authData = JToken.Parse(authJson);
					v2AccessToken = authData["access_token"].ToString();
					v2AccessTokenExpiration = DateTime.Now.AddSeconds(authData["expires_in"].Value<int>());
				}
			}

			if (!string.IsNullOrEmpty(v2AccessToken))
			{
				try
				{
					string json = await Web.DownloadString($"https://osu.ppy.sh/api/v2/{request.Address}", v2AccessToken);

					dynamic deserializedObject = JsonConvert.DeserializeObject(json, request.ReturnType, settings: new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
					//if (request.ShouldReturnSingle)
					//	return deserializedObject[0];

					return deserializedObject;
				}
				catch (Exception ex)
				{
					Log.Error(ex.InnerMessageIfAny());
					return null;
				}
			}
			return null;
		}
	}
}
