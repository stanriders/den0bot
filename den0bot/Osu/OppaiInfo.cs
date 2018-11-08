// den0bot (c) StanR 2018 - MIT License
using Newtonsoft.Json;

namespace den0bot.Osu
{
	public class OppaiInfo
	{
		[JsonProperty("version")]
		public string version;

		[JsonProperty("max_combo")]
		public short max_combo;

		[JsonProperty("num_circles")]
		public short num_circles;

		[JsonProperty("num_sliders")]
		public short num_sliders;

		[JsonProperty("num_spinners")]
		public short num_spinners;

		[JsonProperty("stars")]
		public double stars;

		[JsonProperty("speed_stars")]
		public double speed;

		[JsonProperty("aim_stars")]
		public double aim;

		[JsonProperty("pp")]
		public double pp;
	}
}
