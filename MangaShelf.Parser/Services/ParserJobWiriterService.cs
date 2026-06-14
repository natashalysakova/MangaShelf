using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using MangaShelf.Parser.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Parser.Services;

public class ParserJobWiriterService : IParserJobWriterService
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;

    public ParserJobWiriterService(IDbContextFactory<MangaSystemDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task RecordError(Guid runId, string url, string json, Exception exception, CancellationToken token)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var run = context.Runs.FirstOrDefault(r => r.Id == runId);
        if (run == null)
        {
            throw new Exception($"No run found with id {runId}");
        }

        run.Errors.Add(new ParserError
        {
            Url = url,
            VolumeJson = json,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            RunTime = DateTimeOffset.Now
        });

        await context.SaveChangesAsync();
    }

    public async Task RecordError(Guid runId, string url, Exception exception, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var run = context.Runs
            .Include(x=>x.ParserStatus)
            .FirstOrDefault(r => r.Id == runId);

        if (run == null)
        {
            throw new Exception($"No run found with id {runId}");
        }

        if(run.Type == ParserRunType.SingleUrl)
        {
            run.Status = RunStatus.Error;
            run.Finished = DateTimeOffset.Now;
            run.Progress = -1;
            run.ParserStatus.Status = ParserStatus.Idle;
        }

        run.Errors.Add(new ParserError
        {
            Url = url,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            RunTime = DateTimeOffset.Now
        });

        await context.SaveChangesAsync(token);
    }

    public async Task RecordErrorAndStop(Guid runId, Exception? exception, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var run = context.Runs
                    .Include(x => x.ParserStatus)
                    .FirstOrDefault(r => r.Id == runId);

        if (run == null)
        {
            throw new Exception($"No run found with id {runId}");
        }


        run.Status = RunStatus.Error;
        run.Finished = DateTimeOffset.Now;
        run.Progress = -1;

        run.ParserStatus.Status = ParserStatus.Idle;


        await context.SaveChangesAsync();
    }


    public async Task<bool> RunJob(Guid jobId, CancellationToken token)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var job = context.Runs.Include(x => x.ParserStatus).SingleOrDefault(x => x.Id == jobId);

        if (job == null)
        {
            return false;
        }

        job.Status = RunStatus.Running;
        job.ParserStatus.Status = ParserStatus.GatheringVolumes;
        job.Started = DateTimeOffset.Now;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task SetToParsingStatus(Guid runId, IEnumerable<string> volumes, CancellationToken token)
    {
        await SetStatusInternal(runId, ParserStatus.Parsing, volumes, token);
    }

    public async Task SetProgress(Guid runId, double progress, string url, bool isUpdated, CancellationToken token)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var parserStatus = context.Parsers
            .Include(ps => ps.Jobs)
                .ThenInclude(r => r.Errors)
            .FirstOrDefault(ps => ps.Jobs.Any(r => r.Id == runId));

        if (parserStatus == null)
        {
            throw new Exception($"No parser status found for run id {runId}");
        }

        var run = parserStatus.Jobs.FirstOrDefault(r => r.Id == runId);
        if (run == null)
        {
            throw new Exception($"No run found with id {runId}");
        }

        run.Progress = progress;
        if(isUpdated)
        {
            run.VolumesUpdated.Add(url);
        }
        else
        {
            run.VolumesAdded.Add(url);
        }

        context.Entry(run).State = EntityState.Modified;

        await context.SaveChangesAsync(token);
    }

    public async Task SetToFinishedStatus(Guid runId, CancellationToken token)
    {
        await SetStatusInternal(runId, ParserStatus.Idle, null, token);
    }

    private async Task SetStatusInternal(Guid runId, ParserStatus status, IEnumerable<string>? volumes, CancellationToken token)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var parserStatus = context.Parsers
            .Include(ps => ps.Jobs)
                .ThenInclude(r => r.Errors)
            .FirstOrDefault(ps => ps.Jobs.Any(r => r.Id == runId));

        if (parserStatus == null)
        {
            throw new Exception($"No parser status found for run id {runId}");
        }

        var run = parserStatus.Jobs.FirstOrDefault(r => r.Id == runId);
        if (run == null)
        {
            throw new Exception($"No run found with id {runId}");
        }

        parserStatus.Status = status;

        if(status == ParserStatus.Parsing && volumes is not null)
        {
           run.VolumesFound = volumes.Count();
        }

        if (status == ParserStatus.Idle)
        {
            run.Status = RunStatus.Finished;
            run.Finished = DateTimeOffset.Now;
            run.Progress = 100.0;
        }

        await context.SaveChangesAsync(token);

    }

    public async Task RunSingleJob(Guid jobId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var job = context.Runs.Include(x => x.ParserStatus).SingleOrDefault(x => x.Id == jobId);

        if (job == null || job.Type != ParserRunType.SingleUrl)
        {
            return;
        }
        job.VolumesFound = 1;
        job.Status = RunStatus.Running;
        job.ParserStatus.Status = ParserStatus.Parsing;
        job.Started = DateTimeOffset.Now;

        await context.SaveChangesAsync();
    }

    public async Task SetSingleJobToFinishedStatus(Guid jobId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var job = await context.Runs.Include(x => x.ParserStatus).SingleOrDefaultAsync(x => x.Id == jobId);
        if (job == null || job.Type != ParserRunType.SingleUrl)
        {
            return;
        }

        job.ParserStatus.Status = ParserStatus.Idle;

        job.Status = RunStatus.Finished;
        job.Finished = DateTimeOffset.Now;
        job.Progress = 100.0;

        await context.SaveChangesAsync();
    }

    public async Task SetSingleJobToErrorStatus(Guid jobId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var job = await context.Runs.Include(x => x.ParserStatus).SingleOrDefaultAsync(x => x.Id == jobId);
        if (job == null || job.Type != ParserRunType.SingleUrl)
        {
            return;
        }

        job.Status = RunStatus.Error;
        job.Finished = DateTimeOffset.Now;
        job.Progress = -1;

        job.ParserStatus.Status = ParserStatus.Idle;

        await context.SaveChangesAsync();
    }
}
