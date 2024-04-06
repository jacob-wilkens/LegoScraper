using LegoScraper.Interfaces;

namespace LegoScraper.Models
{
    /// <summary>
    /// Represents a record of a Lego item.
    /// </summary>
    public class LegoRecord : ICsvRecord
    {
        /// <summary>
        /// Gets or sets the item number of the Lego item.
        /// </summary>
        public string? ItemNumber { get; set; }

        /// <summary>
        /// Gets or sets the condition of the Lego item.
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Gets or sets the new condition value of the Lego item.
        /// </summary>
        public string? New { get; set; }

        /// <summary>
        /// Gets or sets the used condition value of the Lego item.
        /// </summary>
        public string? Used { get; set; }

        public LegoRecord()
        {
            ItemNumber ??= string.Empty;
            Condition ??= string.Empty;
            New ??= string.Empty;
            Used ??= string.Empty;
        }
    }
}