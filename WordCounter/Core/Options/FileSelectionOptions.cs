namespace WordCounter.Core.Options;

public sealed class FileSelectionOptions
{
    public string Directory { get; init; } = "";
    public IReadOnlyList<string> Patterns { get; init; } = ["*.txt"];
    public bool Recursive { get; init; } = false;
    public int? MaxFiles { get; init; } = null;

    public IReadOnlySet<string> ExcludedFileNames { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}