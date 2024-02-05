// den0bot (c) StanR 2024 - MIT License
using den0bot;
using den0bot.DB;
using den0bot.Types;
using den0bot.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU", false);

string modulePath = Path.GetDirectoryName(AppContext.BaseDirectory) + Path.DirectorySeparatorChar + "Modules";

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
	services.AddHostedService<Bot>();

	if (Config.Params.Modules == null)
	{
		Log.Error("Module list not found!");
		return;
	}

	List<Assembly> allAssemblies = new List<Assembly>();
	if (Directory.Exists(modulePath))
	{
		foreach (string dll in Directory.GetFiles(modulePath, "*.dll"))
			allAssemblies.Add(Assembly.LoadFile(dll));
	}

	foreach (var moduleName in Config.Params.Modules)
	{
		// if it's local
		Type type = Type.GetType($"den0bot.Modules.{moduleName}", false);
		if (type == null)
		{
			// if its not local
			foreach (var ass in allAssemblies)
			{
				// we only allow subclasses of IModule and only if they're in the config
				type = ass.GetTypes().FirstOrDefault(t =>
					t.IsPublic && t.IsSubclassOf(typeof(IModule)) && t.Name == moduleName);

				if (type != null)
					break;
			}
		}

		if (type != null)
		{
			//var module = (IModule)Activator.CreateInstance(type);
			//if (module != null && module.Init())
			{
				services.AddSingleton(typeof(IModule), type);
			}
		}
		else
		{
			Log.Error($"{moduleName} not found!");
		}
	}
}

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(ConfigureServices)
	.UseSerilog((_, _, logger) =>
	{
		logger
#if DEBUG
			.MinimumLevel.Debug()
#endif
			.Enrich.WithProperty("Application", "den0bot")
			.Enrich.FromLogContext()
			.WriteTo.File(@"./logs/log.txt", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 6)
			.WriteTo.Console()
			.WriteTo.Seq("http://seq:5341");
	})
	.Build();

if (!API.Connect())
	return;

await using var db = new Database();
db.Database.EnsureCreated();

await host.RunAsync();