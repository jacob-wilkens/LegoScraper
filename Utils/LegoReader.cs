using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LegoScraper.Models;

namespace LegoScraper.Utils
{
    public static class LegoReader
    {
        public static List<CsvRecord> ReadCsvData(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<CsvRecord>().ToList();
        }
    }
}