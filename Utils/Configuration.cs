using LegoScraper.Interfaces;
using LegoScraper.Models;
using LegoScraper.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Polly;
using Polly.Retry;
using Serilog;
using Serilog.Events;

namespace LegoScraper.Utils
{
    public static class Configuration
    {
        public static Serilog.ILogger SetupLogging(IScraperWindow scraperWindow)
        {
            return new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .Enrich.FromLogContext()
                  .WriteTo.Sink(new ScraperSink(scraperWindow))
                  .Filter.ByExcluding(FilterLogMessage)
                  .CreateLogger();
        }

        public static ResiliencePipeline SetupResiliencePipeline(Microsoft.Extensions.Logging.ILogger logger)
        {
            var maxRetryAttempts = 2;

            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions()
                {
                    ShouldHandle = new PredicateBuilder()
                  .Handle<NoSuchElementException>()
                  .Handle<WebDriverException>()
                  .Handle<StaleElementReferenceException>(),
                    MaxRetryAttempts = maxRetryAttempts,
                    OnRetry = args =>
                    {
                        logger.LogDebug("Retrying for {itemNumber} attempt {retryCount} of {maxRetryAttempts} for {exception}", args.Context.Properties.GetValue(ResilienceKeys.ItemNumber, ""), args.AttemptNumber, maxRetryAttempts, args.Outcome.Exception);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(20))
                .Build();
        }

        private static bool FilterLogMessage(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();

            return message.Contains("Hosting environment") || message.Contains("Content root path");
        }

        public static ChromeDriver SetupChromeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--log-level=3");
            options.AddArgument("--no-sandbox");

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;

            var driver = new ChromeDriver(service, options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return driver;
        }
    }
}