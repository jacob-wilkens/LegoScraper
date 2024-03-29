using LegoScraper.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LegoScraper.Services;

public class Worker(ILogger<Worker> logger, IWatcherService watcher, Queue queue, IProcessor processor) : BackgroundService
{
  private readonly ILogger<Worker> _logger = logger;
  private readonly IWatcherService _watcher = watcher;
  private readonly Queue _queue = queue;
  private readonly IProcessor _processor = processor;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _watcher.Start();
    while (!stoppingToken.IsCancellationRequested)
    {
      var @event = await _queue.Consume(stoppingToken);
      await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
      _logger.LogInformation("[{date}] {fileName} arrived to worker.", $"{DateTime.Now:O}", @event.Name);

      try
      {
        _processor.ProcessData(@event.FullPath, stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "[{date}] {fileName} could not be processed by worker.", $"{DateTime.Now:O}", @event.Name);
        continue;
      }

      _logger.LogInformation("[{date}] {fileName} processed by worker.", $"{DateTime.Now:O}", @event.Name);
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogDebug("Worker stopping at: {time}", DateTimeOffset.Now);
    await base.StopAsync(cancellationToken);
  }
}