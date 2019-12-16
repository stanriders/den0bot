// den0bot (c) StanR 2019 - MIT License

using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace den0bot.Util
{
	public static class Web
	{
		private static readonly HttpClient client = new HttpClient();

		public static async Task<string> DownloadString(string address) => await client.GetStringAsync(address);

		public static async Task<byte[]> DownloadBytes(string address) => await client.GetByteArrayAsync(address);

		public static async Task<Stream> DownloadStream(string address) => await client.GetStreamAsync(address);

		public static async Task<string> PostJson(string address, string json)
		{
			var response = await client.PostAsync(address, new StringContent(json, Encoding.UTF8, "application/json"));
			if (response.IsSuccessStatusCode)
				return await response.Content.ReadAsStringAsync();

			return string.Empty;
		}
	}
}