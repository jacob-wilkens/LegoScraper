namespace LegoScraper.Interfaces
{
    /// <summary>
    /// Represents a record in a CSV file.
    /// </summary>
    public interface ICsvRecord
    {
        /// <summary>
        /// Gets or sets the item number in the CSV record.
        /// </summary>
        string ItemNumber { get; set; }

        /// <summary>
        /// Gets or sets the condition of the item in the CSV record.
        /// </summary>
        string Condition { get; set; }
    }
}