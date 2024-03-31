using LegoScraper.Interfaces;
using Serilog;
using Serilog.Events;

namespace LegoScraper.Services
{
    public class ScraperWindowLogger(IScraperWindow scraperWindow) : ILogger
    {
        private readonly IScraperWindow _scraperWindow = scraperWindow;

        public void Write(LogEvent logEvent)
        {
            _scraperWindow.LogContent(logEvent);
        }
    }
}