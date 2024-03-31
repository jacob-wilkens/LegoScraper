using LegoScraper.Interfaces;
using Serilog.Core;
using Serilog.Events;

namespace LegoScraper.Services
{
    public class ScraperSink(IScraperWindow scraperWindow) : ILogEventSink
    {
        private readonly IScraperWindow _scraperWindow = scraperWindow;

        public void Emit(LogEvent logEvent)
        {
            _scraperWindow.LogContent(logEvent);
        }
    }
}