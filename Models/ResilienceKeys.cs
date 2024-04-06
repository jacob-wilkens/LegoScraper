using Polly;

namespace LegoScraper.Models
{
    /// <summary>
    /// Provides keys for resilience properties.
    /// </summary>
    public static class ResilienceKeys
    {
        /// <summary>
        /// Represents the key for the item number property.
        /// </summary>
        public static readonly ResiliencePropertyKey<string> ItemNumber = new("ItemNumber");
    }
}