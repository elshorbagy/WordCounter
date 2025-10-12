using System.Text.RegularExpressions;
using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class RegexWordExtractor : IWordExtractor
{
    private static readonly Regex WordRx = new(@"[A-Za-z]+(?:'[A-Za-z]+)?", RegexOptions.Compiled);

    public IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (Match m in WordRx.Matches(text))
            yield return m.Value;
    }
}
