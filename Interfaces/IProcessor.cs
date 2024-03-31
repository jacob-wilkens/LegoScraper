namespace LegoScraper.Interfaces
{
    public interface IProcessor
    {
        public bool ProcessData(string fileName, string path, CancellationToken token);
    }
}