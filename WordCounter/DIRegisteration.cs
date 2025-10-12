using Microsoft.Extensions.DependencyInjection;
using WordCounter.Core.Interfaces;
using WordCounter.Core.Options;
using WordCounter.Core.Services;
using WordCounter.IO.FileWriting;

namespace WordCounter;

public class DIRegisteration
{
    public static ServiceProvider RegisterServices(string excludePath)
    {
         var services = new ServiceCollection()        
            .AddSingleton<IFileSystem, LocalFileSystem>()
            .AddSingleton<INormalizer, BasicNormalizer>()
            .AddSingleton<IWordExtractor, RegexWordExtractor>()
        
            .AddSingleton<IExclusionProvider>(sp =>
                new FileExclusionProvider(
                    sp.GetRequiredService<IFileSystem>(),
                    sp.GetRequiredService<INormalizer>(),
                    excludePath))
            .AddSingleton<IWordCounter, WordCounterService>()
            .AddSingleton<IResultWriter, AlphabeticalResultWriter>()
            .AddSingleton(new FileSelectionOptions
            {
                Patterns = ["*.txt"],
                Recursive = false,                
                ExcludedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        Path.GetFileName(excludePath)
                    }
            });

        return services.BuildServiceProvider();
    }
}
