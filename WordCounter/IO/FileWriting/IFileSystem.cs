namespace WordCounter.IO.FileWriting;

public interface IFileSystem
{
    Task<IReadOnlyList<string>> GetTextFilesAsync(string dir, int maxFiles);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    Task WriteAllLinesAsync(string path, IEnumerable<string> lines);
    Task EnsureDirectoryAsync(string dir);
    Task<bool> FileExistsAsync(string path);
}
