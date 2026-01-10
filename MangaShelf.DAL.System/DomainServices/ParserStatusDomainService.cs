//using MangaShelf.DAL.DomainServices;
//using MangaShelf.DAL.Models;
//using MangaShelf.DAL.System.Interfaces;
//using MangaShelf.DAL.System.Models;
//using Microsoft.EntityFrameworkCore;

//namespace MangaShelf.DAL.System.DomainServices;

//public class ParserStatusDomainService : BaseDomainService<MangaSystemDbContext, ParserStatus>, IParsedStatusDomainService
//{
//    internal ParserStatusDomainService(MangaSystemDbContext context) : base(context)
//    {
//    }

//    public ParserStatus GetOrCreateParserByName(string name)
//    {
//        var parser = _context.Parsers
//            .Include(p => p.Runs)
//                .ThenInclude(r => r.Errors)
//            .Where(p => p.ParserName == name).SingleOrDefault();

//        if (parser == null)
//        {
//            parser = CreateNewParser(name);
//        }

//        return parser;
//    }

//    public ParserStatus CreateNewParser(string name)
//    {
//        return new ParserStatus
//        {
//            ParserName = name,
//            LastRun = DateTime.Now,
//            Status = Status.Idle
//        };
//    }

//    public ParserStatus? GetRunById(Guid runId)
//    {
//        return _context.Parsers
//            .Include(p=>p.Runs)
//                .ThenInclude(r=>r.Errors)
//            .Where(p => p.Runs
//                .Any(r => r.Id == runId)).SingleOrDefault();
//    }

//    public ParserStatus SetErrorAndStop(Guid runId, Exception? exception)
//    {
//        var parserStatus = GetRunById(runId);

//        var run = parserStatus!.Runs.Single(r => r.Id == runId);

//        AddError(run, exception: exception);

//        run.Status = RunStatus.Error;
//        run.Finished = DateTimeOffset.Now;
//        run.Progress = -1;

//        parserStatus.Status = Status.Idle;

//        return parserStatus;
//    }


//    private ParserRun AddError(ParserRun run, string? url = null, string? json = null, string? errorMessage = null, Exception? exception = null)
//    {
//        run.Errors.Add(new ParserError
//        {
//            ExceptionMessage = exception?.Message,
//            Url = url,
//            VolumeJson = json,
//            ErrorMessage = errorMessage,
//            StackTrace = exception?.StackTrace,
//            RunTime = DateTimeOffset.Now
//        });

//        return run;
//    }

//    public ParserStatus SetProgress(Guid runId, double progress)
//    {
//        var parserStatus = GetRunById(runId);

//        var run = parserStatus!.Runs.Single(r => r.Id == runId);

//        run.Progress = progress;

//        return parserStatus;

//    }

//    public ParserStatus SetStatus(Guid runId, Status status)
//    {
//        var parserStatus = GetRunById(runId);

//        var run = parserStatus!.Runs.Single(r => r.Id == runId);

//        parserStatus.Status = status;

//        if (status == Status.Idle)
//        {
//            run.Status = RunStatus.Finished;
//            run.Finished = DateTimeOffset.Now;
//        }
//        else if (status == Status.GatheringVolumes)
//        {
//            run.Status = RunStatus.Started;
//            run.Started = DateTimeOffset.Now;
//        }

//        return parserStatus;
//    }

//    public ParserStatus SetError(Guid runId, string url, Exception exception)
//    {
//        var parserStatus = GetRunById(runId);

//        var run = parserStatus!.Runs.Single(r => r.Id == runId);

//        AddError(run, url, exception: exception);

//        return parserStatus;
//    }

//    public ParserRun StartNewRun()
//    {
//        return new ParserRun
//        {
//            Id = Guid.NewGuid(),
//            Started = DateTimeOffset.Now,
//            Status = RunStatus.Started,
//            Progress = 0
//        };
//    }
//}
