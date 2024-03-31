

using Serilog.Events;

namespace LegoScraper.Interfaces
{
    public interface IScraperWindow
    {
        void Refresh(int percent, string message);
        void LogContent(LogEvent logEvent);
    }
}