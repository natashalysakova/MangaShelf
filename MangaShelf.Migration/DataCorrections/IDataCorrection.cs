using MangaShelf.DAL;

namespace MangaShelf.Migration.DataCorrections;

public interface IDataCorrection
{
    Task ApplyCorrection(MangaDbContext context);
}
