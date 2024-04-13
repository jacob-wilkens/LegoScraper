using LegoScraper.Models;
using Spectre.Console;

namespace LegoScraper.Interfaces
{
    /// <summary>
    /// Defines a processor that can process data.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Processes data from a file.
        /// </summary>
        /// <param name="fileName">The name of the file to process.</param>
        /// <param name="data">The data to process.</param>
        /// <param name="task">The progress task associated with the data processing.</param>
        /// <param name="token">A cancellation token that can be used to cancel the data processing.</param>
        Task ProcessData(string fileName, List<CsvRecord> data, ProgressTask task, CancellationToken token);
    }
}