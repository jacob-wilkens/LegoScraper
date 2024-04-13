using LegoScraper.Interfaces;
using LegoScraper.Models;
using LegoScraper.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Spectre.Console;

namespace LegoScraper.Services
{
    public class Processor(ILogger<Processor> logger, ILegoClient legoClient) : IProcessor
    {
        private readonly ILogger<Processor> _logger = logger;
        private readonly ILegoClient _legoClient = legoClient;

        public async Task ProcessData(string fileName, List<CsvRecord> data, ProgressTask task, CancellationToken token)
        {
            var isMiniFig = fileName.Contains("mini-fig");

            var file = fileName.Replace(".csv", "_updated.csv");
            using var writer = new StreamWriter(file, false);

            writer.WriteLine("Item Number,Condition,New,Used");
            writer.Flush();

            var _pipeline = Configuration.SetupResiliencePipeline(_logger);
            var context = ResilienceContextPool.Shared.Get(token);

            var progressTotal = data.Count;
            var progress = 0;

            foreach (var record in data)
            {
                if (token.IsCancellationRequested) break;

                context.Properties.Set(ResilienceKeys.ItemNumber, record.ItemNumber);

                task.State.Update<ProgressTaskStruct>("item", _ =>
                {
                    _.ItemNumber = record.ItemNumber;
                    _.CurrentIteration = (progress + 1).ToString();
                    _.TotalIterations = progressTotal.ToString();
                    return _;
                });

                try
                {
                    var entry = await _pipeline.ExecuteAsync(async (ctx) => await GetData(isMiniFig, record), context);
                    writer.WriteLine($"{entry.ItemNumber},{entry.Condition},{entry.New},{entry.Used}");
                    writer.Flush();
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("{ex}", ex);
                    _logger.LogError("Error processing record {ItemNumber}", record.ItemNumber);
                }

                progress += 1;
                task.Value = progress * 100 / progressTotal;
            }

            ResilienceContextPool.Shared.Return(context);
        }

        private async Task<LegoRecord> GetData(bool isMiniFig, CsvRecord record)
        {
            var data = new LegoRecord
            {
                ItemNumber = record.ItemNumber,
                Condition = record.Condition,
                New = Constants.EmptyRecord,
                Used = Constants.EmptyRecord
            };

            var url = (isMiniFig ? Constants.GetMiniFigUri(record.ItemNumber) : Constants.GetLegoSetUri(record.ItemNumber)).ToString();

            string newPrice = Constants.EmptyRecord;
            string usedPrice = Constants.EmptyRecord;

            switch (record.Condition)
            {
                case "B":
                    var prices = await GetPrices(url, record.ItemNumber, record.Condition);
                    newPrice = prices[0];
                    usedPrice = prices[1];
                    break;
                case "N":
                    newPrice = await GetNewPrice(url, record.ItemNumber, record.Condition);
                    break;
                case "U":
                    usedPrice = await GetUsedPrice(url, record.ItemNumber, record.Condition);
                    break;
            }

            data.New = newPrice;
            data.Used = usedPrice;

            return data;
        }

        private async Task<List<string>> GetPrices(string url, string itemNumber, string condition)
        {
            var prices = await _legoClient.Scrape(url, condition);
            _logger.LogDebug("Item Number {itemNumber} New price: {newPrice}, Used price: {usedPrice}", itemNumber, prices[0], prices[1]);

            return prices;
        }

        private async Task<string> GetNewPrice(string url, string itemNumber, string condition)
        {
            var newPrice = await _legoClient.Scrape(url, condition);
            _logger.LogDebug("Item Number {itemNumber}: New price: {newPrice}", itemNumber, newPrice);

            return newPrice.FirstOrDefault() ?? Constants.EmptyRecord;
        }

        private async Task<string> GetUsedPrice(string url, string itemNumber, string condition)
        {
            var usedPrice = await _legoClient.Scrape(url, condition);
            _logger.LogDebug("Item Number {itemNumber}: Used price: {usedPrice}", itemNumber, usedPrice);

            return usedPrice.FirstOrDefault() ?? Constants.EmptyRecord;
        }
    }

    public struct ProgressTaskStruct(string value)
    {
        public string ItemNumber { get; set; } = value ?? throw new ArgumentNullException(nameof(value));
        public string CurrentIteration { get; set; }
        public string TotalIterations { get; set; }
    }
}