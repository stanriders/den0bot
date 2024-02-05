// den0bot (c) StanR 2021 - MIT License
using den0bot.Modules.Osu.Util;
using den0bot.Types;
using Microsoft.Extensions.Logging;
using Serilog;

namespace den0bot.Modules.Osu.Types
{
	public class OsuModule : IModule
	{
		private readonly ILogger<IModule> logger;
		protected OsuModule(ILogger<IModule> logger) : base(logger)
		{
			this.logger = logger;
		}

		public override bool Init()
		{
			if (string.IsNullOrEmpty(Config.Params.osuToken) || string.IsNullOrEmpty(Config.Params.osuClientId) || string.IsNullOrEmpty(Config.Params.osuClientSecret))
			{
				logger.LogError("osu! API Key is not defined!");
				return false;
			}

			return base.Init();
		}
	}
}
