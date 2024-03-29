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

        public CsvRecord()
        {
            ItemNumber ??= string.Empty;
            Condition ??= string.Empty;
        }
    }
}