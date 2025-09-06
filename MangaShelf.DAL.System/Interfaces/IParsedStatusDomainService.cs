using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.DAL.System.Interfaces;

public interface IParsedStatusDomainService : IDomainService<ParserStatus>, ISystemDomainService
{
    ParserStatus CreateNewParser(string name);
    ParserStatus GetOrCreateParserByName(string name);
    ParserStatus? GetRunById(Guid runId);
    ParserStatus SetError(Guid runId, string url, Exception exception);
    ParserStatus SetErrorAndStop(Guid runId, Exception? exception);
    ParserStatus SetProgress(Guid runId, double progress);
    ParserStatus SetStatus(Guid runId, ParserStatus status);
    ParserJob StartNewRun();
}
