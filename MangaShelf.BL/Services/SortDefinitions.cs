namespace MangaShelf.BL.Services;

public class SortDefinitions<T>
{
    public Func<T, object> SortFunction { get; set; }
    public bool Descending { get; set; }
}
