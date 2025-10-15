using Microsoft.Extensions.DependencyInjection;
using WordCounter;
using WordCounter.Core.Interfaces;
using WordCounter.Enums;
using WordCounter.Extensions;

IFileSystem fs;

IExclusionProvider exProvider;

IWordCounter counter;

IResultWriter resultWriter;

var argsDict = args.ToDictionary();
if (!argsDict.TryGetValue("--input", out var inputDir) || string.IsNullOrWhiteSpace(inputDir))
{
    Console.Error.WriteLine("Usage: dotnet run -- --input <dir> [--out <dir>] [--exclude <file>]");
    return (int)ExitCode.InvalidArgs;
}

var outDir = argsDict.TryGetValue("--out", out var o) ? o! : Path.Combine(inputDir, "out");
var excludePath = argsDict.TryGetValue("--exclude", out var e) ? e! : Path.Combine(inputDir, "exclude.txt");

RegisterServices(excludePath, out fs, out exProvider, out counter, out resultWriter);

try
{
    // 1) Load files (max 4 for demo)
    var files = await fs.GetTextFilesAsync(inputDir, maxFiles: 4);
    if (files.Count == 0)
    {
        Console.Error.WriteLine("No input files matched the selection options.");
        return (int)ExitCode.NoFiles;
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
    return (int)ExitCode.Success;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return (int)ExitCode.UnhandledError;
}

static void RegisterServices(string excludePath, out IFileSystem fs, out IExclusionProvider exProvider, out IWordCounter counter, out IResultWriter resultWriter)
{
    var sp = DIRegisteration.RegisterServices(excludePath);

    fs = sp.GetRequiredService<IFileSystem>();
    exProvider = sp.GetRequiredService<IExclusionProvider>();
    counter = sp.GetRequiredService<IWordCounter>();
    resultWriter = sp.GetRequiredService<IResultWriter>();
}