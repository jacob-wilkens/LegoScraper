using Serilog.Core;
using Serilog.Events;
using Spectre.Console;

namespace LegoScraper.Services
{
    public class ScraperSink() : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            var color = GetColorString(logEvent.Level);
            var message = logEvent.RenderMessage();
            var timeMessage = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

            var messageMarkup = new Markup($"{timeMessage}) {message}", new Style(color));

            if (logEvent.Level == LogEventLevel.Error && logEvent.Exception != null)
            {
                AnsiConsole.WriteException(logEvent.Exception, ExceptionFormats.ShortenEverything);
            }

            AnsiConsole.Write(messageMarkup);
            AnsiConsole.WriteLine();
        }

        private static Color GetColorString(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Debug => Color.Blue,
                LogEventLevel.Information => Color.White,
                LogEventLevel.Warning => Color.Yellow,
                LogEventLevel.Error => Color.Red,
                LogEventLevel.Fatal => Color.Red,
                _ => Color.White
            };
        }
    }
}