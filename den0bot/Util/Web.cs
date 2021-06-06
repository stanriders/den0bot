// den0bot (c) StanR 2021 - MIT License
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace den0bot.Util
{
	public static class Web
	{
		private static readonly HttpClient client = new();

		public static Task<byte[]> DownloadBytes(string address) => client.GetByteArrayAsync(address);

		public static Task<Stream> DownloadStream(string address) => client.GetStreamAsync(address);

		public static Task<string> DownloadString(string address) => client.GetStringAsync(address);

		public static async Task<string> DownloadString(string address, string bearer)
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri(address),
				Method = HttpMethod.Get,
				Headers =
				{
					{HttpRequestHeader.Authorization.ToString(), $"Bearer {bearer}"}
				}
			};

			var response = await client.SendAsync(request);
			if (response.IsSuccessStatusCode)
				return await response.Content.ReadAsStringAsync();

			if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.Unauthorized)
			{
				request = new HttpRequestMessage
				{
					RequestUri = new Uri(response.RequestMessage.RequestUri.ToString()),
					Method = HttpMethod.Get,
					Headers =
					{
						{HttpRequestHeader.Authorization.ToString(), $"Bearer {bearer}"}
					}
				};

				response = await client.SendAsync(request);
				if (response.IsSuccessStatusCode)
					return await response.Content.ReadAsStringAsync();
			}

			return string.Empty;
		}

		public static async Task<string> PostJson(string address, string json)
		{
			var response = await client.PostAsync(address, new StringContent(json, Encoding.UTF8, "application/json"));
			if (response.IsSuccessStatusCode)
				return await response.Content.ReadAsStringAsync();

			return string.Empty;
		}
	}
}