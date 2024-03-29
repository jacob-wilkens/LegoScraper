namespace LegoScraper.Interfaces
{
    public interface IProcessor
    {
        public void ProcessData(string path, CancellationToken token);
    }
}