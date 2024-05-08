
internal static class StringExtensions
{
    internal static string RemoveSymbols(this string str)
    {
        return str
            .Replace(".", String.Empty)
            .Replace(",", String.Empty)
            .Replace("\"", String.Empty)
            .Replace("(", String.Empty)
            .Replace(")", String.Empty)
            .Replace("[", String.Empty)
            .Replace("]", String.Empty)
            .Replace("/", String.Empty)
            .Replace("!", String.Empty)
            .Replace("?", String.Empty)
            .Replace("$", String.Empty)
            .Replace("#", String.Empty)
            .Replace("%", String.Empty)
            .Replace("-", String.Empty)
            .Replace("@", String.Empty);
    }

    internal static string[] SplitHyphens(this string str)
    {
        return str.Split('-', StringSplitOptions.RemoveEmptyEntries);
    }
}
