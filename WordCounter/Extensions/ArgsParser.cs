namespace WordCounter.Extensions;

static class ArgsParser
{
    public static Dictionary<string, string?> ToDictionary(this string[] args)
    {
        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith("--"))
            {
                string? val = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : null;
                map[a] = val;
            }
        }
        return map;
    }
}
