using MangaShelf.DAL;

namespace MangaShelf.Infrastructure.Seed;

public interface IDataCorrection
{
    Task ApplyCorrection(MangaDbContext context);
}
