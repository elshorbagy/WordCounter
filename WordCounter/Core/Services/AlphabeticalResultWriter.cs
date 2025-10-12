using WordCounter.Core.Interfaces;
using WordCounter.IO.FileWriting;

namespace WordCounter.Core.Services;

public sealed class AlphabeticalResultWriter(IFileSystem fs) : IResultWriter
{
    private readonly IFileSystem _fs = fs;

    public async Task WriteAsync(string outputDir, IReadOnlyDictionary<string, int> counts)
    {
        var buckets = new Dictionary<char, List<(string word, int count)>>();
        for (char c = 'A'; c <= 'Z'; c++) buckets[c] = new();

        foreach (var kv in counts)
        {
            var word = kv.Key;
            var first = word[0];
            if (first < 'A' || first > 'Z')
                continue;

            buckets[first].Add((word, kv.Value));
        }

        foreach (var letter in buckets.Keys)
        {
            var lines = buckets[letter]
                .OrderByDescending(x => x.count)
                .ThenBy(x => x.word, StringComparer.Ordinal)
                .Select(x => $"{x.word} {x.count}");

            var path = Path.Combine(outputDir, $"FILE_{letter}.txt");
            await _fs.WriteAllLinesAsync(path, lines);
        }
    }
}
