using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.ComponentModel;
using Serilog;

//Dependency injection, serilog, settings
//lots of chained methods!
namespace ConsoleUI
{
    class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder); //add app settings to config

            Log.Logger = new LoggerConfiguration() //set up logger
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Starting Application");

            var host = Host.CreateDefaultBuilder() //override event source
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IGreetingService, GreetingService>();
              
                })
                .UseSerilog()
                .Build();

            var svc = ActivatorUtilities.CreateInstance<GreetingService>(host.Services);
            svc.Run();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) //has to use appsettings, not optional, reload if changed
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true) //e.g. development environment, if not specified, production
                .AddEnvironmentVariables();
        }

    }
}