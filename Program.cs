using LegoScraper.Interfaces;
using LegoScraper.Services;
using LegoScraper.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = Configuration.SetupLogging();

using var host = new HostBuilder()
         .ConfigureServices((hostContext, services) =>
         {
           services.AddHostedService<Worker>();
           services.AddSingleton(new HttpClient());
           services.AddSingleton<IWatcherService, WatcherService>();
           services.AddSingleton<ILegoClient, LegoClient>();
           services.AddSingleton<Queue>();
           services.AddSingleton<IProcessor, Processor>();
         })
        .UseSerilog()
        .Build();

try
{
  await host.RunAsync();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}