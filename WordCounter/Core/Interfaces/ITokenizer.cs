namespace WordCounter.Core.Interfaces;

public interface ITokenizer
{
    IEnumerable<string> Tokenize(string text);
}