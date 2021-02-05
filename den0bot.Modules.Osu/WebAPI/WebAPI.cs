﻿// den0bot (c) StanR 2021 - MIT License
using System;
using System.Threading.Tasks;
using den0bot.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace den0bot.Modules.Osu.WebAPI
{
	public static class WebApiHandler
	{
		private static string v2AccessToken = string.Empty;
		private static DateTime v2AccessTokenExpiration = DateTime.Now;

		public static async Task<Out> MakeApiRequest<In, Out>(IRequest<In,Out> request)
		{
			return request.API switch
			{
				APIVersion.V1 => await V1ApiRequest(request),
				APIVersion.V2 => await V2ApiRequest(request),
				_ => throw new NotImplementedException(),
			};
		}

		private static async Task<Out> V1ApiRequest<In, Out>(IRequest<In, Out> request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken))
			{
				Log.Error("API Key is not defined!");
				return default(Out);
			}

			try
			{
				string json =
					await Web.DownloadString($"https://osu.ppy.sh/api/{request.Address}&k={Config.Params.osuToken}");

				var deserializedObject = JsonConvert.DeserializeObject<In>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
				if (deserializedObject != null)
					return request.Process(deserializedObject);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return default(Out);
		}

		private static async Task<Out> V2ApiRequest<In, Out>(IRequest<In, Out> request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuClientId) || string.IsNullOrEmpty(Config.Params.osuClientSecret))
			{
				Log.Error("API Key is not defined!");
				return default(Out);
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

					var deserializedObject = JsonConvert.DeserializeObject<In>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
					if (deserializedObject != null)
						return request.Process(deserializedObject);
				}
				catch (Exception ex)
				{
					Log.Error(ex.InnerMessageIfAny());
				}
			}
			return default(Out);
		}
	}
}
