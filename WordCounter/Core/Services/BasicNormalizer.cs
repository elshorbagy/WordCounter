using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class BasicNormalizer : INormalizer
{
    public string NormalizeToken(string token)
        => token.Trim().ToUpperInvariant();
}
