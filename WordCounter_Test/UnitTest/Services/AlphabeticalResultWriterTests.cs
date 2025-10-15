using NSubstitute;
using WordCounter.Core.Interfaces;
using WordCounter.Core.Services;

namespace WordCounter_Test.UnitTest.Services;

[TestClass]
public class AlphabeticalResultWriterTests
{
    private static IFileSystem CreateFs() => Substitute.For<IFileSystem>();

    [TestMethod]
    public async Task WriteAsync_Creates_26_Files_One_Per_Alphabet_Letter()
    {
        // Arrange
        var fs = CreateFs();
        var sut = new AlphabeticalResultWriter(fs);
        var output = "out";
        var counts = new Dictionary<string, int>
        {
            ["APPLE"] = 2,
            ["ZOO"] = 1 
        };

        // Act
        await sut.WriteAsync(output, counts);
        
        await fs.Received(26).WriteAllLinesAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>());
        
        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_A.txt"),
            Arg.Any<IEnumerable<string>>());

        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_Z.txt"),
            Arg.Any<IEnumerable<string>>());
        
        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_B.txt"),
            Arg.Is<IEnumerable<string>>(seq => !seq.Any()));
    }

    [TestMethod]
    public async Task WriteAsync_Puts_Words_Into_Correct_Buckets()
    {
        // Arrange
        var fs = CreateFs();
        var sut = new AlphabeticalResultWriter(fs);
        var output = "out";
        var counts = new Dictionary<string, int>
        {
            ["ALPHA"] = 3,
            ["ALMOND"] = 1,
            ["BRAVO"] = 2,
            ["CHARLIE"] = 5
        };

        // Act
        await sut.WriteAsync(output, counts);

        // Assert A
        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_A.txt"),
            Arg.Is<IEnumerable<string>>(seq => seq.SequenceEqual(new[]
            {
                // Sorted by count desc then word asc (ordinal)
                "ALPHA 3",
                "ALMOND 1"
            })));

        // Assert B
        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_B.txt"),
            Arg.Is<IEnumerable<string>>(seq => seq.SequenceEqual(new[]
            {
                "BRAVO 2"
            })));

        // Assert C
        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_C.txt"),
            Arg.Is<IEnumerable<string>>(seq => seq.SequenceEqual(new[]
            {
                "CHARLIE 5"
            })));
    }

    [TestMethod]
    public async Task WriteAsync_Sorts_By_Count_Desc_Then_Word_Asc_Ordinal()
    {
        // Arrange
        var fs = CreateFs();
        var sut = new AlphabeticalResultWriter(fs);
        var output = "out";
        
        var counts = new Dictionary<string, int>
        {
            ["APPLE"] = 2,
            ["AXE"] = 5,
            ["ALPHA"] = 5,
            ["AARDVARK"] = 5,
            ["AZURE"] = 2,
            ["ANT"] = 2
        };

        // Act
        await sut.WriteAsync(output, counts);

        // Assert
        var expectedA = new[]
        {
            "AARDVARK 5",
            "ALPHA 5",
            "AXE 5",
            "ANT 2",
            "APPLE 2",
            "AZURE 2"
        };

        await fs.Received(1).WriteAllLinesAsync(
            Path.Combine(output, "FILE_A.txt"),
            Arg.Is<IEnumerable<string>>(seq => seq.SequenceEqual(expectedA)));
    }
}
