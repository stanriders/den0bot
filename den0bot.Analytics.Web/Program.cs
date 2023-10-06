// den0bot (c) StanR 2023 - MIT License
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace den0bot.Analytics.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog((context, services, configuration) => configuration
					.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
					.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
					.MinimumLevel.Override("Default", LogEventLevel.Debug)
					.MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
					.Enrich.FromLogContext()
					.Enrich.WithProperty("Application", "den0bot.Analytics.Web")
					.Enrich.WithClientIp("CF-Connecting-IP")
					.Enrich.WithRequestHeader("CF-IPCountry")
					.Enrich.WithRequestHeader("Referer")
					.Enrich.WithRequestHeader("User-Agent")
					.WriteTo.Console()
					.WriteTo.File("./logs/log_analytics.txt", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 6)
					.WriteTo.Seq("http://seq:5341")
					.ReadFrom.Services(services))
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
	}
}