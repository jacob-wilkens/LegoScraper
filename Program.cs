using LegoScraper.Interfaces;
using LegoScraper.Services;
using LegoScraper.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;

Log.Logger = Configuration.SetupLogging();

try
{
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

  host.Run();
}
catch (Exception ex)
{
  // Ignore WebDriverException
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
  if (ex.GetType().IsAssignableFrom(typeof(WebDriverException))) ex = null;

  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}