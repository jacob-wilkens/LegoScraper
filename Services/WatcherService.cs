using System.Text.RegularExpressions;
using LegoScraper.Interfaces;
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
        public List<string> Files { get; } = [];

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
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name == null || e.ChangeType != WatcherChangeTypes.Created || Files.Any(i => e.Name.Contains(i))) return;

            if (!CsvExtension.IsMatch(e.Name))
            {
                _logger.LogWarning("Cannot process the file because it is not a CSV and does not match the pattern 'sets*.csv' or 'mini-fig*.csv'. File : {file}", e.Name);
                return;
            }

            Files.Add(e.Name.Replace(".csv", ""));
            _queue.Produce(e).Wait();
        }
    }
}