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

        public void ProcessData(string path)
        {
            var isMiniFig = path.Contains("mini-fig");
            var data = ReadCsvData(path).ToArray();

            if (data == null || data.Length == 0)
            {
                _logger.LogWarning("No data to process.");
                return;
            }

            var file = isMiniFig ? Constants.MiniFigCsvFile : Constants.LegoSetCsvFile;
            using var writer = new StreamWriter(file, false);

            writer.WriteLine("ItemNumber,Condition,Value");
            writer.Flush();

            var _pipeline = Configuration.SetupResiliencePipeline(_logger);
            var context = ResilienceContextPool.Shared.Get();

            for (int i = 0; i < data.Length; i++)
            {
                var record = data[i];
                context.Properties.Set(ResilienceKeys.ItemNumber, record.ItemNumber);
                try
                {
                    record.Value = _pipeline.Execute((ctx) => GetData(isMiniFig, record, i), context);
                    writer.WriteLine($"{record.ItemNumber},{record.Condition},{record.Value}");
                    writer.Flush();
                    Thread.Sleep(2_500);
                }
                catch (Exception)
                {
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

        private string GetData(bool isMiniFig, CsvRecord record, int index)
        {
            var url = isMiniFig ? Constants.GetMiniFigUri(record.ItemNumber) : Constants.GetLegoSetUri(record.ItemNumber);
            _driver.Navigate().GoToUrl(url);

            if (_driver.PageSource.Contains("Item Not Found"))
            {
                _logger.LogDebug("Item not found: {itemNumber}", record.ItemNumber);
                return "N/A";
            }

            var price = isMiniFig ? MiniFigPage.Scrape(_driver, record.Condition, index) : LegoSetPage.Scrape(_driver, record.Condition, index);
            _logger.LogDebug("Price: {price}", price);

            return price;
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