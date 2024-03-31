using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LegoScraper.Interfaces;
using LegoScraper.Models;
using LegoScraper.Pages;
using LegoScraper.Utils;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Polly;

namespace LegoScraper.Services
{
    public class Processor(ILogger<Processor> logger, IWebDriver driver, IScraperWindow scraperWindow) : IProcessor
    {
        private readonly ILogger<Processor> _logger = logger;
        private readonly IWebDriver _driver = driver;
        private readonly IScraperWindow _scraperWindow = scraperWindow;
        private bool ClickedCookieButton { get; set; } = false;

        public bool ProcessData(string fileName, string path, CancellationToken token)
        {
            var isMiniFig = path.Contains("mini-fig");
            List<CsvRecord> data;

            try
            {
                data = ReadCsvData(path);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("{ex}", ex);
                _logger.LogError("Error reading CSV file {path}", path);
                return false;
            }

            if (data == null || data.Count == 0)
            {
                _logger.LogWarning("No data to process.");
                return false;
            }

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
                _scraperWindow.Refresh(progress * 100 / progressTotal, $"Processing {record.ItemNumber} ({progress + 1} of {progressTotal})");

                try
                {
                    var entry = _pipeline.Execute((ctx) => GetData(isMiniFig, record), context);
                    writer.WriteLine($"{entry.ItemNumber},{entry.Condition},{entry.New},{entry.Used}");
                    writer.Flush();
                    Thread.Sleep(2_500);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("{ex}", ex);
                    _logger.LogError("Error processing record {ItemNumber}", record.ItemNumber);
                }

                progress += 1;
            }

            ResilienceContextPool.Shared.Return(context);

            var success = progress == progressTotal;

            if (success) _scraperWindow.Refresh(100, $"Processing complete for {fileName}.");

            return progress == progressTotal;
        }

        private LegoRecord GetData(bool isMiniFig, CsvRecord record)
        {
            var data = new LegoRecord
            {
                ItemNumber = record.ItemNumber,
                Condition = record.Condition,
                New = Constants.EmptyRecord,
                Used = Constants.EmptyRecord
            };

            var url = isMiniFig ? Constants.GetMiniFigUri(record.ItemNumber) : Constants.GetLegoSetUri(record.ItemNumber);
            _driver.Navigate().GoToUrl(url);

            // Click the cookie button if it hasn't been clicked yet
            if (!ClickedCookieButton)
            {
                Web.WaitAndClick(_driver, Constants.CookieButton);
                ClickedCookieButton = true;
            }

            if (_driver.PageSource.Contains("Item Not Found") || _driver.PageSource.Contains("No Item(s) were found. Please try again!"))
            {
                _logger.LogDebug("Item not found: {itemNumber}", record.ItemNumber);
                return data;
            }

            string newPrice = Constants.EmptyRecord;
            string usedPrice = Constants.EmptyRecord;

            switch (record.Condition)
            {
                case "B":
                    var prices = GetPrices(record.ItemNumber, isMiniFig, record.Condition);
                    newPrice = prices[0];
                    usedPrice = prices[1];
                    break;
                case "N":
                    newPrice = GetNewPrice(record.ItemNumber, isMiniFig, record.Condition);
                    break;
                case "U":
                    usedPrice = GetUsedPrice(record.ItemNumber, isMiniFig, record.Condition);
                    break;
            }

            data.New = newPrice;
            data.Used = usedPrice;

            return data;
        }

        private List<string> GetPrices(string itemNumber, bool isMiniFig, string condition)
        {
            var prices = isMiniFig ? MiniFigPage.Scrape(_driver, condition) : LegoSetPage.Scrape(_driver, condition);
            _logger.LogDebug("Item Number {i}: New price: {newPrice}, Used price: {usedPrice}", itemNumber, prices[0], prices[1]);

            return prices;
        }

        private string GetNewPrice(string itemNumber, bool isMiniFig, string condition)
        {
            var newPrice = (isMiniFig ? MiniFigPage.Scrape(_driver, condition) : LegoSetPage.Scrape(_driver, condition))[0];
            _logger.LogDebug("Item Number {itemNumber}: New price: {newPrice}", itemNumber, newPrice);

            return newPrice;
        }

        private string GetUsedPrice(string itemNumber, bool isMiniFig, string condition)
        {
            var usedPrice = (isMiniFig ? MiniFigPage.Scrape(_driver, condition) : LegoSetPage.Scrape(_driver, condition))[0];
            _logger.LogDebug("Item Number {itemNumber}: Used price: {usedPrice}", itemNumber, usedPrice);

            return usedPrice;
        }

        private static List<CsvRecord> ReadCsvData(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<CsvRecord>().ToList();
        }
    }
}