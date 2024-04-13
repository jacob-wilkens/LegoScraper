namespace LegoScraper.Interfaces
{
    public interface ILegoClient
    {
        public Task<List<string>> Scrape(string url, string condition);
    }
}