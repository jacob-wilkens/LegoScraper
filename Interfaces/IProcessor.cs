using Spectre.Console;

namespace LegoScraper.Interfaces
{
    public interface IProcessor
    {
        public void ProcessData(string fileName, string path, ProgressTask task, CancellationToken token);
    }
}