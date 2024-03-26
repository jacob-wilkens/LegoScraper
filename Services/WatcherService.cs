using System.Text.RegularExpressions;
using LegoScraper.Interfaces;
using LegoScraper.Utils;
using Microsoft.Extensions.Logging;

namespace LegoScraper.Services
{
    public class WatcherService : IWatcherService
    {
        private readonly ILogger _logger;
        private readonly FileSystemWatcher _watcher;
        private readonly Queue _queue;
        private static string Name => nameof(WatcherService);
        private static Regex CsvExtension = new Regex(@"(sets|mini-figs).*\.csv$");

        public WatcherService(ILogger<WatcherService> logger, Queue queue)
        {
            _logger = logger;
            _queue = queue;
            _watcher = new FileSystemWatcher
            {
                Path = Directory.GetCurrentDirectory(),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                Filter = "*.csv"
            };

            _watcher.Created += Created;

            _logger.LogDebug($"[{DateTime.Now:O}] {Name}: Instance created.");
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
            _logger.LogDebug($"[{DateTime.Now:O}] {Name}: Started.");
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created || e.Name == Constants.LegoSetCsvFile || e.Name == Constants.MiniFigCsvFile) return;

            if (!CsvExtension.IsMatch(e.Name!))
            {
                _logger.LogWarning($"Cannot process the file because it is not a CSV and does not match the pattern 'sets*.csv' or 'mini-fig*.csv'. File : {e.Name}");
                return;
            }

            _logger.LogInformation($"[{DateTime.Now:O}] {Name}: {e.Name} arrived to watcher.");
            _queue.Produce(e).Wait();
        }
    }
}