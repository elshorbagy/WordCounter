using WordCounter.Core.Options;

namespace WordCounter.IO.FileWriting;

public sealed class LocalFileSystem(FileSelectionOptions? options = null) : IFileSystem
{
    private readonly FileSelectionOptions _options = options ?? new FileSelectionOptions();

    public Task<IReadOnlyList<string>> GetTextFilesAsync(string dir, int maxFiles)
    {
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException(dir);

        var searchOption = _options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        IEnumerable<string> paths = [];
        foreach (var pattern in _options.Patterns)
        {
            var batch = Directory.EnumerateFiles(dir, pattern, searchOption);
            paths = paths.Concat(batch);
        }
        
        paths = paths.Where(p => !_options.ExcludedFileNames.Contains(Path.GetFileName(p)));
        
        paths = paths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase);

        if (maxFiles > 0)
            paths = paths.Take(maxFiles);

        var list = paths.ToList();
        return Task.FromResult<IReadOnlyList<string>>(list);
    }

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);

    public Task WriteAllTextAsync(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        return File.WriteAllTextAsync(path, content);
    }

    public Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        return File.WriteAllLinesAsync(path, lines);
    }

    public Task EnsureDirectoryAsync(string dir)
    {
        Directory.CreateDirectory(dir);
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string path) => Task.FromResult(File.Exists(path));
}
