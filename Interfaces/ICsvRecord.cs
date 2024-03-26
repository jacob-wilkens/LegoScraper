namespace LegoScraper.Interfaces
{
    public interface ICsvRecord
    {
        string ItemNumber { get; set; }
        string Condition { get; set; }
        string? Value { get; set; }
    }
}