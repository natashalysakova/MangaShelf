namespace MangaShelf.BL.Parsers;

[Serializable]
public class LastPageReachedException : Exception
{
    public LastPageReachedException() { }
    public LastPageReachedException(string url) : base($"Last page reached for URL: {url}") { }
    public LastPageReachedException(string url, Exception inner) : base($"Last page reached for URL: {url}", inner) { }
}
