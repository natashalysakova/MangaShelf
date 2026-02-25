using MangaShelf.DAL.System.Models;

namespace MangaShelf.BL.Services;

public interface ISettingReadService
{
    Task<IEnumerable<Settings>> GetAll(CancellationToken token = default);
    Task<IDictionary<string, IEnumerable<Settings>>> GetAllGrouped(CancellationToken token = default);
    Task<IEnumerable<Settings>> GetAllFromSection(string section, CancellationToken token = default);
    Task<Settings?> GetByKey(string section, string key, CancellationToken token = default);
    Task<Settings?> GetById(Guid id, CancellationToken token = default);
}
