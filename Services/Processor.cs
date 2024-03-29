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
    public class Processor(ILogger<Processor> logger, IWebDriver driver) : IProcessor
    {
        private readonly ILogger<Processor> _logger = logger;
        private readonly IWebDriver _driver = driver;
        private bool ClickedCookieButton { get; set; } = false;

        public void ProcessData(string path)
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
                return;
            }

            if (data == null || data.Count == 0)
            {
                _logger.LogWarning("No data to process.");
                return;
            }

            var file = isMiniFig ? Constants.MiniFigCsvFile : Constants.LegoSetCsvFile;
            using var writer = new StreamWriter(file, false);

            writer.WriteLine("Item Number,Condition,New,Used");
            writer.Flush();

            var _pipeline = Configuration.SetupResiliencePipeline(_logger);
            var context = ResilienceContextPool.Shared.Get();

            foreach (var record in data)
            {
                context.Properties.Set(ResilienceKeys.ItemNumber, record.ItemNumber);
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
            }

            ResilienceContextPool.Shared.Return(context);
        }

        public void Close()
        {
            _driver.Close();
            _driver.Dispose();
        }

        private LegoRecord GetData(bool isMiniFig, CsvRecord record)
        {
            var data = new LegoRecord
            {
                ItemNumber = record.ItemNumber,
                Condition = record.Condition,
                New = "N/A",
                Used = "N/A"
            };

            var url = isMiniFig ? Constants.GetMiniFigUri(record.ItemNumber) : Constants.GetLegoSetUri(record.ItemNumber);
            _driver.Navigate().GoToUrl(url);

            // Click the cookie button if it hasn't been clicked yet
            if (!ClickedCookieButton)
            {
                Web.WaitAndClick(_driver, Constants.CookieButton);
                ClickedCookieButton = true;
            }

            if (_driver.PageSource.Contains("Item Not Found"))
            {
                _logger.LogDebug("Item not found: {itemNumber}", record.ItemNumber);
                return data;
            }

            string newPrice = "N/A";
            string usedPrice = "N/A";

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