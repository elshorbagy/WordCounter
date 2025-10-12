using System.Collections.Concurrent;
using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class WordCounterService(INormalizer norm, ITokenizer tok) : IWordCounter
{
    private readonly INormalizer _norm = norm;
    private readonly ITokenizer _tok = tok;

    public (Dictionary<string, int> counts, long excludedTotal)
        Count(IEnumerable<string> texts, IReadOnlySet<string> excluded)
    {        
        var inputs = texts as string[] ?? texts.ToArray();

        var total = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        long excludedTotal = 0;

        Parallel.ForEach(inputs, text =>
        {
            long localExcluded = 0;

            foreach (var raw in _tok.Tokenize(text))
            {
                var t = _norm.NormalizeToken(raw);
                if (t.Length == 0) continue;

                if (excluded.Contains(t)) { localExcluded++; continue; }

                total.AddOrUpdate(t, 1, static (_, old) => old + 1);
            }

            Interlocked.Add(ref excludedTotal, localExcluded);
        });

        // ارجاع Dictionary عادي زي التوقيع الأصلي
        return (new Dictionary<string, int>(total, total.Comparer), excludedTotal);
    }
}
