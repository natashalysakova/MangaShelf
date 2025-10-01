namespace MangaShelf.BL.Interfaces
{
    public interface ICacheService
    {
        IEnumerable<string> GetSearchAutoComplete();
    }
}
