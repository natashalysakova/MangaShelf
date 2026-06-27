using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.BL.Services.Parsing;

public class ParseJobManagerService : IParseJobManagerService
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<ParseJobManagerService> _logger;
    private readonly JobManagerSettings _options;

    public ParseJobManagerService(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        IConfigurationService configurationService,
        ILogger<ParseJobManagerService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _options = configurationService.JobManager;
    }

    public async Task CancelJob(Guid jobId, CancellationToken token)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var runningJob = await context.Runs.Include(r => r.ParserStatus).SingleOrDefaultAsync(r => r.Id == jobId);

        if (runningJob != null)
        {
            runningJob.Status = RunStatus.Cancelled;
            runningJob.Finished = DateTimeOffset.Now;
            runningJob.Progress = -1;
            runningJob.ParserStatus.Status = ParserStatus.Idle;
        }

        await context.SaveChangesAsync(token);
    }

    public async Task<int> CreateScheduledJobs(CancellationToken token)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var currentTime = DateTimeOffset.Now;
        var allParsers = await dbContext.Parsers.ToListAsync(token);
        var parsersToRun = allParsers.Where(x => x.NextRun <= currentTime);

        foreach (var parser in parsersToRun)
        {
            var job = CreateJobInternal(parser, ParserRunType.FullSite);
            parser.Jobs.Add(job);
            parser.NextRun = DateTimeOffset.Now + _options.DelayBetweenRuns;
        }

        await dbContext.SaveChangesAsync(token);

        return parsersToRun.Count();
    }

    public async Task<Guid> CreateSingleJob(string parserName, string url, CancellationToken token)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var parser = dbContext.Parsers.SingleOrDefault(p => p.ParserName == parserName);
        if (parser == null)
        {
            throw new Exception($"No parser found with name {parserName}");
        }

        var job = CreateJobInternal(parser, ParserRunType.SingleUrl, url);
        parser.Jobs.Add(job);

        await dbContext.SaveChangesAsync(token);
        return job.Id;
    }

    public async Task<Guid> CreateParserJob(Guid parserId, CancellationToken token)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var parser = dbContext.Parsers.Find(parserId);
        if (parser == null)
        {
            throw new Exception($"No parser found with id {parserId}");
        }

        var job = CreateJobInternal(parser, ParserRunType.FullSite, null);
        parser.Jobs.Add(job);

        await dbContext.SaveChangesAsync(token);
        return job.Id;
    }

    public async Task InitializeParsers(IEnumerable<string> parsers, CancellationToken token)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync(token);
        foreach (var parserName in parsers)
        {
            var parser = await context.Parsers.FirstOrDefaultAsync(p => p.ParserName == parserName, token);
            if (parser is not null)
            {
                if (_options.ResetNextRun)
                {
                    parser.NextRun = DateTimeOffset.Now;
                }
                continue;
            }

            context.Parsers.Add(new ParserModel
            {
                ParserName = parserName,
                Status = ParserStatus.Idle,
                NextRun = DateTimeOffset.Now
            });
        }

        await ResetStuckJobs(context);
        await context.SaveChangesAsync(token);
    }

    private async Task ResetStuckJobs(MangaSystemDbContext context)
    {
        try
        {
            var parserStatuses = context.Parsers
                .Include(p => p.Jobs)
                    .ThenInclude(r => r.Errors);

            var notFinishedProperly = parserStatuses
                .SelectMany(x => x.Jobs)
                .Where(r => r.Status == RunStatus.Waiting || r.Status == RunStatus.Running);

            foreach (var job in notFinishedProperly)
            {
                job.Status = RunStatus.Error;
                job.Finished = DateTimeOffset.Now;
                job.Progress = -1;
                job.Errors.Add(new ParserError()
                {
                    ErrorMessage = "Was automatically cancelled after restart",
                    RunTime = job.Finished.Value
                });
            }

            foreach (var parser in parserStatuses)
            {
                parser.Status = ParserStatus.Idle;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // do nothing, we don't want to block the app from starting
        }
    }

    public async Task<IEnumerable<Guid>> PrepareWaitingJobs(CancellationToken token)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        var jobs = await dbContext.Runs
               .Include(r => r.ParserStatus)
               .Where(r => r.Status == RunStatus.Waiting)
               .OrderBy(r => r.Created)
               .ToListAsync(token);

        var parsers = jobs.Select(x => x.ParserStatus).Distinct();
        foreach (var parser in parsers)
        {
            parser.Status = ParserStatus.Waiting;
        }

        await dbContext.SaveChangesAsync(token);

        return jobs.Select(j => j.Id);
    }

    private ParserJob CreateJobInternal(ParserModel parser, ParserRunType parserRunType, string? url = null)
    {
        return new ParserJob()
        {
            Created = DateTimeOffset.Now,
            Progress = 0,
            Status = RunStatus.Waiting,
            Type = parserRunType,
            Url = url
        };
    }

    public async Task RecordError(Guid jobId, Exception exception, string? url = null, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var run = context.Runs
            .Include(x => x.ParserStatus)
            .Single(r => r.Id == jobId);

        run.Errors.Add(new ParserError
        {
            Url = url,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            RunTime = DateTimeOffset.Now
        });

        await context.SaveChangesAsync(token);
    }

    public async Task RecordError(Guid jobId, string url, string json, Exception exception, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var run = context.Runs.Single(r => r.Id == jobId);

        run.Errors.Add(new ParserError
        {
            Url = url,
            VolumeJson = json,
            ExceptionMessage = exception?.Message,
            StackTrace = exception?.StackTrace,
            RunTime = DateTimeOffset.Now
        });

        await context.SaveChangesAsync(token);
    }

    public async Task RecordErrorAndStop(Guid jobId, Exception exception, string? url = null, CancellationToken token = default)
    {
        await RecordError(jobId, exception, url, token);
        await SetToErrorStatus(jobId, token);
    }

    public async Task SetProgress(Guid runId, double progress, ParseResult? result, CancellationToken token)
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

        if (result != null)
        {
            var volumeReference = new VolumeReference
            {
                VolumeId = result.VolumeReference.VolumeId,
                FullName = result.VolumeReference.FullName,
                PublicId = result.VolumeReference.PublicId,
            };

            if (result.State == State.Added)
            {
                volumeReference.AddedParserJobId = run.Id;
                run.AddedVolumes.Add(volumeReference);
            }
            else if (result.State == State.Updated)
            {
                volumeReference.UpdatedParserJobId = run.Id;
                run.UpdatedVolumes.Add(volumeReference);
            }
        }

        context.Entry(run).State = EntityState.Modified;

        await context.SaveChangesAsync(token);
    }

    public async Task SetToFinishedStatus(Guid jobId, CancellationToken token = default)
    {
        await SetStatusInternal(jobId, RunStatus.Finished, token: token);
    }

    public async Task SetToErrorStatus(Guid jobId, CancellationToken token = default)
    {
        await SetStatusInternal(jobId, RunStatus.Error, token: token);
    }

    public async Task SetToCancelledStatus(Guid jobId, CancellationToken token)
    {
        await SetStatusInternal(jobId, RunStatus.Cancelled, token: token);
    }

    public async Task SetToParsingStatus(Guid jobId, IEnumerable<string> volumesToParse, CancellationToken token = default)
    {
        await SetStatusInternal(jobId, RunStatus.Running, volumesToParse.Count(), token);
    }

    public async Task SetStatusInternal(Guid jobId, RunStatus status, int volumesCount = 0, CancellationToken token = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var parserStatus = context.Parsers
            .Include(ps => ps.Jobs)
                .ThenInclude(r => r.Errors)
            .FirstOrDefault(ps => ps.Jobs.Any(r => r.Id == jobId));

        if (parserStatus == null)
        {
            throw new Exception($"No parser status found for run id {jobId}");
        }

        var run = parserStatus.Jobs.FirstOrDefault(r => r.Id == jobId);
        if (run == null)
        {
            throw new Exception($"No run found with id {jobId}");
        }

        if (run.Type == ParserRunType.SingleUrl && status == RunStatus.GatheringVolumes)
        {
            // skip gathering volumes for single url runs, as we don't gather volumes for single url runs, we just parse the single url directly
            run.Status = RunStatus.Running;
        }
        else
        {
            run.Status = status;
        }

        switch (status)
        {
            case RunStatus.Waiting:
                break;
            case RunStatus.GatheringVolumes:
                parserStatus.Status = ParserStatus.GatheringVolumes;
                run.Started = DateTimeOffset.Now;
                run.Progress = 0;
                break;
            case RunStatus.Running when run.Type == ParserRunType.FullSite:
                parserStatus.Status = ParserStatus.Parsing;
                run.VolumesFound = volumesCount;
                break;
            case RunStatus.Running when run.Type == ParserRunType.SingleUrl:
                parserStatus.Status = ParserStatus.Parsing;
                run.Progress = 0;
                run.Started = DateTimeOffset.Now;
                run.VolumesFound = 1;
                break;
            case RunStatus.Finished:
                parserStatus.Status = ParserStatus.Idle;
                run.Finished = DateTimeOffset.Now;
                run.Progress = 100;
                parserStatus.NextRun = DateTimeOffset.Now.Add(_options.DelayBetweenRuns);
                break;
            case RunStatus.Error or RunStatus.Cancelled:
                parserStatus.Status = ParserStatus.Idle;
                run.Finished = DateTimeOffset.Now;
                run.Progress = -1;
                break;
            default:
                break;
        }

        await context.SaveChangesAsync(token);
    }

    public async Task RunJob(Guid jobId, CancellationToken token = default)
    {
        await SetStatusInternal(jobId, RunStatus.GatheringVolumes, token: token);
    }
}