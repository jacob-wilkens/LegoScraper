using LegoScraper.Interfaces;
using LegoScraper.Models;
using LegoScraper.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

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

      _logger.LogInformation("{fileName} arrived to worker.", @event.Name);
      await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

      List<CsvRecord> data;
      try
      {
        data = LegoReader.ReadCsvData(@event.FullPath);
      }
      catch (Exception ex)
      {
        _logger.LogDebug("{ex}", ex);
        _logger.LogError("Error reading CSV file {path}", @event.FullPath);
        continue;
      }

      if (data == null || data.Count == 0)
      {
        _logger.LogWarning("No data to process.");
        continue;
      }

      var progress = AnsiConsole.Progress();
      progress.RenderHook = Configuration.RenderProgress;

      try
      {
        await progress
        .Columns(
        [
          new TaskDescriptionColumn(),
          new ProgressBarColumn(),
          new PercentageColumn(),
          new ElapsedTimeColumn(),
          new SpinnerColumn()
        ])
        .StartAsync(async (ctx) =>
        {
          var task = ctx.AddTask(@event.Name!);
          await _processor.ProcessData(@event.Name!, data, task, stoppingToken);
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{fileName} could not be processed by worker.", @event.Name);
      }
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogDebug("Worker stopping");
    await base.StopAsync(cancellationToken);
  }
}