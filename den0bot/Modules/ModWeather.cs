using den0bot.Types;
using den0bot.Util;
using System;
using System.Threading.Tasks;
using den0bot.Types.Answers;
using Newtonsoft.Json;
using Message = Telegram.Bot.Types.Message;
using Serilog;

namespace den0bot.Modules
{
	internal class ModWeather : IModule
	{
		private class WeatherResponse
		{
			public class LocationData
			{
				public string Name { get; set; }
				public string Country { get; set; }
			}
			public class Data
			{
				public class ConditionData
				{
					[JsonProperty("text")]
					public string Text { get; set; }
				}

				[JsonProperty("condition")]
				public ConditionData Condition { get; set; }
				[JsonProperty("temp_c")]
				public double Temperature { get; set; }
				[JsonProperty("wind_kph")]
				public double WindSpeed { get; set; }
				[JsonProperty("wind_dir")]
				public string WindDirection { get; set; }
				public int Humidity { get; set; }
				[JsonProperty("feelslike_c")]
				public double FeelsLike { get; set; }
				[JsonProperty("uv")]
				public double Uv { get; set; }
			}

			[JsonProperty("location")]
			public LocationData Location { get; set; } = new LocationData();

			[JsonProperty("current")] 
			public Data WeatherData { get; set; } = new Data();
		}
		private readonly string apiBase = "https://api.weatherapi.com/v1/current.json?key=" + Config.Params.WeatherToken;

		public ModWeather()
		{
			AddCommand(new Command
			{
				Names = { "weather", "w" },
				Reply = true,
				Slow = true,
				ActionAsync = GetWeather
			});
		}

		private async Task<ICommandAnswer> GetWeather(Message msg)
		{
			var indexOfQuery = msg.Text!.IndexOf(' ');
			if (indexOfQuery == -1)
			{
				return new TextCommandAnswer(Localization.Get("generic_badrequest", msg.Chat.Id));
			}

			var query = msg.Text[indexOfQuery..].Trim();
			var apiLink = $"{apiBase}&q={query}";

			string json;
			try
			{
				json = await Web.DownloadString(apiLink);
			}
			catch (Exception ex)
			{
				Log.Error(ex, ex.InnerMessageIfAny());
				return new TextCommandAnswer(Localization.Get("generic_fail", msg.Chat.Id));
			}

			var weather = JsonConvert.DeserializeObject<WeatherResponse>(json);
			if (weather == null)
			{
				return new TextCommandAnswer(Localization.Get("generic_fail", msg.Chat.Id));
			}

			return new TextCommandAnswer($"{weather.Location.Name}, {weather.Location.Country}\r\n\r\n{weather.WeatherData.Condition.Text}\r\n🌡: {weather.WeatherData.Temperature:N1}C (feels like {weather.WeatherData.FeelsLike:N1}C)\r\n💧: {weather.WeatherData.Humidity:N1}%\r\n💨: {weather.WeatherData.WindSpeed:N1}km/h {MapDirection(weather.WeatherData.WindDirection)}\r\nUV: {weather.WeatherData.Uv:N1}");
		}

		private string MapDirection(string direction)
		{
			return direction switch
			{
				"N" => "⬆️",
				"NE" or "NNE" or "ENE" => "↗️",
				"E" => "➡️",
				"SE" or "SSE" or "ESE" => "↘️",
				"S" => "⬇️",
				"SW" or "SSW" or "WSW" => "↙️",
				"W" => "⬅️",
				"NW" or "NNW" or "WNW" => "↖️",
				_ => "❔"
			};
		}
	}
}
