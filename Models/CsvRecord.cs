using CsvHelper.Configuration.Attributes;
using LegoScraper.Interfaces;

namespace LegoScraper.Models
{
    public class CsvRecord : ICsvRecord
    {
        [Name("Item Number")]
        public string ItemNumber { get; set; }
        [Name("Condition")]
        public string Condition { get; set; }
        [Name("Value")]
        public string? Value { get; set; }

        public CsvRecord()
        {
            ItemNumber ??= string.Empty;
            Value ??= string.Empty;
            Condition ??= string.Empty;
        }
    }
}