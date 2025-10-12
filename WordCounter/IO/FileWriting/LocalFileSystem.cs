namespace WordCounter.IO.FileWriting;

public sealed class LocalFileSystem : IFileSystem
{
    public Task<IReadOnlyList<string>> GetTextFilesAsync(string dir, int maxFiles)
    {
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException(dir);

        var files = Directory.EnumerateFiles(dir, "*.txt", SearchOption.TopDirectoryOnly)
                             .Where(p => !string.Equals(Path.GetFileName(p), "exclude.txt", StringComparison.OrdinalIgnoreCase))
                             .Take(maxFiles)
                             .ToList();
        return Task.FromResult<IReadOnlyList<string>>(files);
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