namespace WordCounter.Core.Interfaces;

public interface IWordExtractor
{
    IEnumerable<string> Tokenize(string text);
}