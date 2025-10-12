namespace WordCounter.Core.Interfaces;

public interface IResultWriter
{
    Task WriteAsync(string outputDir, IReadOnlyDictionary<string, int> counts);
}