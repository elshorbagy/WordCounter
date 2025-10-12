namespace WordCounter.Core.Interfaces;

public interface IExclusionProvider
{
    Task<IReadOnlySet<string>> GetExcludedAsync();
}
