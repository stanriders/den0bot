// den0bot (c) StanR 2024 - MIT License
using System;
using den0bot.Analytics.Data;
using den0bot.Analytics.Web.Caches;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using UAParser;

namespace den0bot.Analytics.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpContextAccessor();

			services.AddDbContext<AnalyticsDatabase>();

			var telegramToken = Configuration["TelegramAPIKey"];
			if (telegramToken == null)
			{
				throw new Exception("Null telegram token!");
			}

			services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(telegramToken));
			services.AddSingleton<ITelegramCache, TelegramCache>();

			services.AddControllersWithViews();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseSerilogRequestLogging(options =>
			{
				options.EnrichDiagnosticContext = (context, httpContext) =>
				{
					var parsedUserAgent = Parser.GetDefault()?.Parse(httpContext.Request.Headers.UserAgent);
					context.Set("Browser", parsedUserAgent?.Browser.ToString());
					context.Set("Device", parsedUserAgent?.Device.ToString());
					context.Set("OS", parsedUserAgent?.OS.ToString());
				};
			});

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseStaticFiles();
			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{action=Index}/{id?}",
					defaults: new { controller = "Home" });
			});
		}
	}
}