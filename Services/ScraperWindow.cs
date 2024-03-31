using System.Text.Json;
using Konsole;
using LegoScraper.Interfaces;
using Serilog.Events;

namespace LegoScraper.Services
{
    public class ScraperWindow : IScraperWindow
    {
        private readonly ConcurrentWriter _window;
        private readonly ProgressBar _progressBar;
        private readonly IConsole ProgressContainer;
        private readonly IConsole LogContainer;

        public ScraperWindow()
        {
            _window = new Window().Concurrent();
            var sections = new Split[]
            {
                new (4, "Progress", LineThickNess.Double, ConsoleColor.DarkGreen),
                new (0, ConsoleColor.Black)
            };

            var consoles = _window.SplitRows(sections);
            ProgressContainer = consoles[0];
            LogContainer = consoles[1];

            _progressBar = new ProgressBar(ProgressContainer, 100);
        }

        public void LogContent(LogEvent e)
        {
            var color = e.Level switch
            {
                LogEventLevel.Information => ConsoleColor.White,
                LogEventLevel.Debug => ConsoleColor.Blue,
                LogEventLevel.Warning => ConsoleColor.Yellow,
                LogEventLevel.Error => ConsoleColor.Red,
                LogEventLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };

            LogContainer.WriteLine(color, e.RenderMessage());
        }

        public void Refresh(int percent, string message)
        {
            _progressBar.Refresh(percent, message);
        }
    }
}