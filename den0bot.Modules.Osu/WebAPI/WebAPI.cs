// den0bot (c) StanR 2021 - MIT License
using System;
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
		private static AccessToken v2AccessToken;

		public static async Task<TOut> MakeApiRequest<TIn, TOut>(IRequest<TIn, TOut> request)
		{
			return request.API switch
			{
				APIVersion.V1 => await V1ApiRequest(request),
				APIVersion.V2 => await V2ApiRequest(request),
				_ => throw new NotImplementedException(),
			};
		}

		private static async Task<TOut> V1ApiRequest<TIn, TOut>(IRequest<TIn, TOut> request)
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

				var deserializedObject = JsonConvert.DeserializeObject<TIn>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
				if (deserializedObject != null)
					return request.Process(deserializedObject);
			}
			catch (Exception ex)
			{
				Log.Error(ex.InnerMessageIfAny());
			}
			return default;
		}

		private static async Task<TOut> V2ApiRequest<TIn, TOut>(IRequest<TIn, TOut> request)
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

			if (v2AccessToken != null)
			{
				try
				{
					string json = await Web.DownloadString($"https://osu.ppy.sh/api/v2/{request.Address}", v2AccessToken.Token);

					var deserializedObject = JsonConvert.DeserializeObject<TIn>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
					if (deserializedObject != null)
						return request.Process(deserializedObject);
				}
				catch (Exception ex)
				{
					Log.Error(ex.InnerMessageIfAny());
				}
			}
			return default;
		}
	}
}
