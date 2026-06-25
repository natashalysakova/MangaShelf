using MangaShelf.DAL;
using System.Linq;
using System.Threading;

namespace MangaShelf.Migration.DataCorrections;

public interface IDataCorrection
{
    Task ApplyCorrection(MangaDbContext context);
}
