using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using ParserModel = MangaShelf.DAL.System.Models.Parser;

namespace MangaShelf.BL.Services.Parsing;

public class ParseJobManagerService : IParseJobManagerService
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly JobManagerSettings _options;

    public ParseJobManagerService(IDbContextFactory<MangaSystemDbContext> dbContextFactory, IConfigurationService configurationService)
    {
        _dbContextFactory = dbContextFactory;
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
        await context.SaveChangesAsync(token);
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
}