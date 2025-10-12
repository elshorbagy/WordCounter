namespace WordCounter.Core.Interfaces;

public interface IWordCounter
{
    (Dictionary<string, int> counts, long excludedTotal)
        Count(IEnumerable<string> texts, IReadOnlySet<string> excluded);
}

