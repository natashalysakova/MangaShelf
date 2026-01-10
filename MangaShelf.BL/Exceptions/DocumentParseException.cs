using AngleSharp.Dom;

namespace MangaShelf.BL.Exceptions;

[Serializable]
public class DocumentParseException : Exception
{
    public IDocument Document { get; }
    public DocumentParseException(string selector, IDocument document) : base(selector)
    {
        Document = document;
    }
}
