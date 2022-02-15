// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Util;
using den0bot.Types;
using Serilog;

namespace den0bot.Modules.Osu.Types
{
	public class OsuModule : IModule
	{
		protected OsuModule() { }

		public override bool Init()
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken) || string.IsNullOrEmpty(Config.Params.osuClientId) || string.IsNullOrEmpty(Config.Params.osuClientSecret))
			{
				Log.Error("osu! API Key is not defined!");
				return false;
			}

			return base.Init();
		}
	}
}
