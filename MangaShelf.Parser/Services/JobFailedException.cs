namespace MangaShelf.Parser.Services;

[Serializable]
internal class JobFailedException : Exception
{
    public string  Url { get; set; }
    public new Exception InnerException { get => base.InnerException!; }
    public JobFailedException(string url, Exception inner) : base($"Job failed for URL: {url}", inner)
    {
        Url = url;
    }
}