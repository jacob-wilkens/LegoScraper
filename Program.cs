using LegoScraper.Interfaces;
using LegoScraper.Services;
using LegoScraper.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;

Log.Logger = Configuration.SetupLogging();

using var host = new HostBuilder()
         .ConfigureServices((hostContext, services) =>
         {
           services.AddHostedService<Worker>();
           services.AddSingleton<IWatcherService, WatcherService>();
           services.AddSingleton<Queue>();
           services.AddSingleton<IProcessor, Processor>();
           services.AddSingleton<IWebDriver, ChromeDriver>((_) => Configuration.SetupChromeDriver());
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