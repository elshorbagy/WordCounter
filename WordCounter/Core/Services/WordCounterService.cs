using System.Collections.Concurrent;
using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class WordCounterService(INormalizer norm, IWordExtractor tok) : IWordCounter
{
    private readonly INormalizer _norm = norm;
    private readonly IWordExtractor _tok = tok;

    public (Dictionary<string, int> counts, long excludedTotal)
        Count(IEnumerable<string> texts, IReadOnlySet<string> excluded)
    {        
        var inputs = texts as string[] ?? [.. texts];

        var total = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        long excludedTotal = 0;

        Parallel.ForEach(inputs, text =>
        {
            long localExcluded = 0;

            foreach (var raw in _tok.Tokenize(text))
            {
                var word = _norm.NormalizeToken(raw);
                if (word.Length == 0) continue;

                if (excluded.Contains(word)) { localExcluded++; continue; }

                total.AddOrUpdate(word, 1, static (_, currentValue) => currentValue + 1);
            }

            Interlocked.Add(ref excludedTotal, localExcluded);
        });
        
        return (new Dictionary<string, int>(total, total.Comparer), excludedTotal);
    }
}
