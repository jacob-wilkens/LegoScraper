using Polly;

namespace LegoScraper.Models
{
    public static class ResilienceKeys
    {
        public static readonly ResiliencePropertyKey<string> ItemNumber = new("ItemNumber");
    }
}