using WordCounter.Core.Options;
using WordCounter.IO.FileWriting;

namespace WordCounter_Test.IntegrationTest.IO;

[TestClass]
public class LocalFileSystemTests
{
    private string _tempDir = null!;

    [TestInitialize]
    public void Setup()
    {        
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TestMethod]
    public async Task GetTextFilesAsync_ShouldReturnOnlyTxtFiles_AndRespectMaxFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDir, "a.txt"), "one");
        File.WriteAllText(Path.Combine(_tempDir, "b.txt"), "two");
        File.WriteAllText(Path.Combine(_tempDir, "c.json"), "{}");

        var fs = new LocalFileSystem();

        // Act
        var files = await fs.GetTextFilesAsync(_tempDir, maxFiles: 2);

        // Assert
        Assert.HasCount(2, files);
        Assert.IsTrue(files.All(f => f.EndsWith(".txt")));
    }

    [TestMethod]
    public async Task GetTextFilesAsync_ShouldExcludeConfiguredFileNames()
    {
        // Arrange
        var excludeFile = "ignore.txt";
        File.WriteAllText(Path.Combine(_tempDir, "keep.txt"), "ok");
        File.WriteAllText(Path.Combine(_tempDir, excludeFile), "skip me");

        var options = new FileSelectionOptions
        {
            Patterns = new[] { "*.txt" },
            ExcludedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { excludeFile }
        };

        var fs = new LocalFileSystem(options);

        // Act
        var files = await fs.GetTextFilesAsync(_tempDir, maxFiles: 10);

        // Assert
        Assert.HasCount(1, files);
        Assert.EndsWith("keep.txt", files.Single());
    }

    [TestMethod]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        var path = Path.Combine(_tempDir, "exists.txt");
        File.WriteAllText(path, "ok");

        var fs = new LocalFileSystem();

        var exists = await fs.FileExistsAsync(path);
        Assert.IsTrue(exists);
    }
}
