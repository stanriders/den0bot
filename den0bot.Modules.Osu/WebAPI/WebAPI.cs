﻿// den0bot (c) StanR 2024 - MIT License
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using den0bot.Modules.Osu.Types.V2;
using den0bot.Modules.Osu.Util;
using den0bot.Util;
using Newtonsoft.Json;
using Serilog;

namespace den0bot.Modules.Osu.WebAPI
{
	public static class WebApiHandler
	{
		private static AccessToken? v2AccessToken;

		public static async Task<TIn?> MakeApiRequest<TIn, TOut>(Request<TIn, TOut> request)
		{
			return request.API switch
			{
				APIVersion.V1 => await V1ApiRequest(request),
				APIVersion.V2 => await V2ApiRequest(request),
				_ => throw new NotImplementedException(),
			};
		}

		private static async Task<TIn?> V1ApiRequest<TIn, TOut>(Request<TIn, TOut> request)
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken))
			{
				Log.Error("API Key is not defined!");
				return default;
			}

			try
			{
				string json =
					await Web.DownloadString($"https://osu.ppy.sh/api/{request.Address}&k={Config.Params.osuToken}");

				return JsonConvert.DeserializeObject<TIn>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return default;
		}

		private static async Task<TIn?> V2ApiRequest<TIn, TOut>(Request<TIn, TOut> request)
		{
			await RefreshToken();

			if (v2AccessToken != null)
			{
				try
				{
					string json;

					if (request.Body is not null)
						json = await Web.PostJson($"https://osu.ppy.sh/api/v2/{request.Address}", request.Body, v2AccessToken.Token, new Dictionary<string, string> {{ "x-api-version", "20240801" } });
					else
						json = await Web.DownloadString($"https://osu.ppy.sh/api/v2/{request.Address}", v2AccessToken.Token, new Dictionary<string, string> { { "x-api-version", "20240801" } });

					return JsonConvert.DeserializeObject<TIn>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
				}
				catch (Exception ex)
				{
					Log.Error(ex.InnerMessageIfAny());
				}
			}
			return default;
		}

		private static async Task RefreshToken()
		{
			if (v2AccessToken == null || v2AccessToken.Expired)
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
					v2AccessToken = JsonConvert.DeserializeObject<AccessToken>(authJson);
				}
			}
		}
	}
}
