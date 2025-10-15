using WordCounter.Core.Interfaces;

namespace WordCounter.Core.Services;

public sealed class FileExclusionProvider(IFileSystem fs, INormalizer norm, string path) : IExclusionProvider
{
    private readonly IFileSystem _fs = fs;
    private readonly INormalizer _norm = norm;
    private readonly string _path = path;

    public async Task<IReadOnlySet<string>> GetExcludedAsync()
    {
        if (!await _fs.FileExistsAsync(_path))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var text = await _fs.ReadAllTextAsync(_path);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tok in text.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries))
            set.Add(_norm.NormalizeToken(tok));
        return set;
    }
}
