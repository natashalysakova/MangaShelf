namespace MangaShelf.Common.Interfaces;

public interface IFilterOptions
{
    int Skip { get; }
    int Take { get; }

    string Search { get; init; }
    OrderBy OrderBy { get; init; }
    bool OrderIsAsc { get; init; }

    ReleaseFilter ReleaseFilter { get; init; }
}

public enum ReleaseFilter
{
    None,
    Released,
    Preorder
}

public enum OrderBy
{
    SeriesTitle,
    Popularity,
    Rating,
    ReleaseDate,
    PreorderDate
}