using LegoScraper.Interfaces;
using LegoScraper.Services;
using LegoScraper.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;

Log.Logger = Configuration.SetupLogging();

var scraperWindow = new ScraperWindow();

using var host = new HostBuilder()
         .ConfigureServices((hostContext, services) =>
         {
           services.AddHostedService<Worker>();
           services.AddSingleton<IWatcherService, WatcherService>();
           services.AddSingleton<Queue>();
           services.AddSingleton<IProcessor, Processor>();
           services.AddSingleton<IWebDriver, ChromeDriver>((_) => Configuration.SetupChromeDriver());
           services.AddSingleton<IScraperWindow>(scraperWindow);
         })
        .UseSerilog(new ScraperWindowLogger(scraperWindow))
        .Build();

var driver = host.Services.GetRequiredService<IWebDriver>();

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
  driver.Quit();
  Log.CloseAndFlush();
}