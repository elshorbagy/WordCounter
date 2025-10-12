using NSubstitute;
using WordCounter.Core.Interfaces;
using WordCounter.Core.Services;
using WordCounter.IO.FileWriting;

namespace WordCounter_Test.UnitTest.Services;

[TestClass]
public class FileExclusionProviderTests
{
    private static (IFileSystem fs, INormalizer norm) Mocks() =>
        (Substitute.For<IFileSystem>(), Substitute.For<INormalizer>());

    [TestMethod]
    public async Task GetExcludedAsync_WhenFileDoesNotExist_ReturnsEmptySet_AndDoesNotReadOrNormalize()
    {
        // Arrange
        var (fs, norm) = Mocks();
        fs.FileExistsAsync("path.txt").Returns(Task.FromResult(false));

        var sut = new FileExclusionProvider(fs, norm, "path.txt");

        // Act
        var set = await sut.GetExcludedAsync();

        // Assert
        Assert.IsEmpty(set);
        await fs.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
        norm.DidNotReceiveWithAnyArgs().NormalizeToken(default!);
    }

    [TestMethod]
    public async Task GetExcludedAsync_SplitsOnWhitespace_NormalizesAndDeduplicates_IgnoringCase()
    {
        // Arrange
        var (fs, norm) = Mocks();
        
        var fileText = "  Hello\thello\nWORLD World\r\n  baz  ";
        fs.FileExistsAsync("ex.txt").Returns(true);
        fs.ReadAllTextAsync("ex.txt").Returns(fileText);

       
        norm.NormalizeToken("Hello").Returns("HELLO");
        norm.NormalizeToken("world").Returns("WORLD");
        norm.NormalizeToken("hello").Returns("HELLO");
        norm.NormalizeToken("HELLO").Returns("HELLO");

        var sut = new FileExclusionProvider(fs, norm, "ex.txt");

        // Act
        var set = await sut.GetExcludedAsync();

        // Assert        
        Assert.HasCount(2, set);
        
        Assert.Contains("HELLO", set);
        Assert.Contains("hello", set);        

        // Verify calls
        await fs.Received(1).ReadAllTextAsync("ex.txt");
        norm.Received(1).NormalizeToken("Hello");
        norm.Received(1).NormalizeToken("hello");
        norm.Received(1).NormalizeToken("WORLD");
        norm.Received(1).NormalizeToken("World");
        norm.Received(1).NormalizeToken("baz");
    }

    [TestMethod]
    public async Task GetExcludedAsync_WhitespaceOnly_ReturnsEmptySet()
    {
        // Arrange
        var (fs, norm) = Mocks();
        fs.FileExistsAsync("ex.txt").Returns(true);
        fs.ReadAllTextAsync("ex.txt").Returns(" \t \r\n   ");

        var sut = new FileExclusionProvider(fs, norm, "ex.txt");

        // Act
        var set = await sut.GetExcludedAsync();

        // Assert
        Assert.IsEmpty(set);
        norm.DidNotReceiveWithAnyArgs().NormalizeToken(default!);
    }

    [TestMethod]
    public async Task GetExcludedAsync_NormalizerCalledOncePerToken_BasedOnOriginalSplit()
    {
        // Arrange
        var (fs, norm) = Mocks();
        fs.FileExistsAsync("ex.txt").Returns(true);
        
        fs.ReadAllTextAsync("ex.txt").Returns("A  BB\tCCC\nD\r\nE");

        norm.NormalizeToken("A").Returns("a");
        norm.NormalizeToken("BB").Returns("bb");
        norm.NormalizeToken("CCC").Returns("ccc");
        norm.NormalizeToken("D").Returns("d");
        norm.NormalizeToken("E").Returns("e");

        var sut = new FileExclusionProvider(fs, norm, "ex.txt");

        // Act
        var set = await sut.GetExcludedAsync();

        // Assert
        Assert.HasCount(5, set);
        Assert.Contains("a", set);
        Assert.Contains("bb", set);
        Assert.Contains("ccc", set);
        Assert.Contains("d", set);
        Assert.Contains("e", set);

        norm.Received(1).NormalizeToken("A");
        norm.Received(1).NormalizeToken("BB");
        norm.Received(1).NormalizeToken("CCC");
        norm.Received(1).NormalizeToken("D");
        norm.Received(1).NormalizeToken("E");
    }

    [TestMethod]
    public async Task GetExcludedAsync_DuplicatesAfterNormalization_AreCollapsed_IgnoringCase()
    {
        // Arrange
        var (fs, norm) = Mocks();
        fs.FileExistsAsync("ex.txt").Returns(true);
        fs.ReadAllTextAsync("ex.txt").Returns("Apple APPLE apple");
        
        norm.NormalizeToken("Apple").Returns("APPLE");
        norm.NormalizeToken("APPLE").Returns("apple");
        norm.NormalizeToken("apple").Returns("ApPlE");

        var sut = new FileExclusionProvider(fs, norm, "ex.txt");

        // Act
        var set = await sut.GetExcludedAsync();

        // Assert
        Assert.HasCount(1, set);        
        Assert.Contains("apple", set);
    }
}
