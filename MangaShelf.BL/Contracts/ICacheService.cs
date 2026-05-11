namespace MangaShelf.BL.Contracts
{
    public interface ICacheService
    {
        IEnumerable<string> GetSearchAutoComplete();
    }
}
