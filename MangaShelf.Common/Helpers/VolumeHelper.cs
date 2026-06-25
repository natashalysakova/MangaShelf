namespace MangaShelf.Common.Helpers;

public static class VolumeHelper
{
    public static string? NormalizedIsbn(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return null;
        }
        return isbn.Replace("-", "").Replace(" ", "").ToUpperInvariant();
    }
}
