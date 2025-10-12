using WordCounter.Core.Interfaces;
using WordCounter.Core.Services;
using WordCounter.Extensions;
using WordCounter.IO.FileWriting;

var argsDict = args.ToDictionary();
if (!argsDict.TryGetValue("--input", out var inputDir) || string.IsNullOrWhiteSpace(inputDir))
{
    Console.Error.WriteLine("Usage: dotnet run -- --input <dir> [--out <dir>] [--exclude <file>]");
    return 1;
}
var outDir = argsDict.TryGetValue("--out", out var o) ? o! : Path.Combine(inputDir, "out");
var excludePath = argsDict.TryGetValue("--exclude", out var e) ? e! : Path.Combine(inputDir, "exclude.txt");

var fs = new LocalFileSystem();
INormalizer normalizer = new BasicNormalizer();
ITokenizer tokenizer = new RegexTokenizer();
IExclusionProvider exProvider = new FileExclusionProvider(fs, normalizer, excludePath);
IWordCounter counter = new WordCounterService(normalizer, tokenizer);
IResultWriter resultWriter = new AlphabeticalResultWriter(fs);

try
{
    // 1) Read up to 4 *.txt (exclude exclude.txt)
    var files = await fs.GetTextFilesAsync(inputDir, maxFiles: 4);
    if (files.Count == 0)
    {
        Console.Error.WriteLine("No .txt files found in input directory.");
        return 2;
    }

    var texts = await Task.WhenAll(files.Select(f => fs.ReadAllTextAsync(f)));

    // 2) Load exclusions
    var excluded = await exProvider.GetExcludedAsync();

    // 3) Count (case-insensitive via normalizer) + total excluded
    var (counts, excludedTotal) = counter.Count(texts, excluded);

    // 4) Write outputs
    await fs.EnsureDirectoryAsync(outDir);
    await resultWriter.WriteAsync(outDir, counts);
    await fs.WriteAllTextAsync(Path.Combine(outDir, "excluded_count.txt"), excludedTotal.ToString());

    Console.WriteLine($"Done. Output → {outDir}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 3;
}

