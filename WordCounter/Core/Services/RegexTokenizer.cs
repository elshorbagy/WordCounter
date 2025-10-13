using System.Text.RegularExpressions;
using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class RegexTokenizer : ITokenizer
{
    private static readonly Regex WordRx = new(@"[A-Za-z]+", RegexOptions.Compiled);

    public IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        foreach (Match m in WordRx.Matches(text))
            yield return m.Value;
    }
}
