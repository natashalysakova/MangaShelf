namespace MangaShelf.BL.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Strips carriage returns, newlines, and tabs from a string, then trims whitespace.
    /// </summary>
    public static string Escaped(this string input)
    {
        if (input is null)
        {
            return string.Empty;
        }

        return input
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty).Trim();
    }

    /// <summary>
    /// Returns true if the string contains any of the given substrings.
    /// </summary>
    public static bool ContainsAny(this string source, IEnumerable<string> toCheck)
    {
        foreach (var check in toCheck)
        {
            if (source.Contains(check))
            {
                return true;
            }
        }
        return false;
    }
}
