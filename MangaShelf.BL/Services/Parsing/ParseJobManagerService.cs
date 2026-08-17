using MangaShelf.BL.Configuration;
using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
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
    private readonly IJobStateTransitionPublisher _stateTransitionPublisher;

    public ParseJobManagerService(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        IConfigurationService configurationService,
        ILogger<ParseJobManagerService> logger,
        IJobStateTransitionPublisher stateTransitionPublisher)
    {
        _logger = logger;
        _options = configurationService.JobManager;
        _stateTransitionPublisher = stateTransitionPublisher;
        _dbContextFactory = dbContextFactory;
    }

    public async Task CancelJob(Guid jobId, CancellationToken token)
    {
        await TransitionJobState(jobId, ParseJobTrigger.CancelJob, context: "User cancelled the job", token: token);
    }

    public async Task<int> CreateScheduledJobs(CancellationToken token)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var currentTime = DateTimeOffset.Now;
        var allParsers = await dbContext.Parsers
            .Include(p => p.Jobs)
            .ToListAsync(token);
        var parsersToRun = allParsers.Where(x => x.NextRun <= currentTime && x.IsActive).ToList();

        _logger.LogDebug("CreateScheduledJobs: Found {TotalParsers} parsers, {ReadyParsers} ready to run. Current time: {CurrentTime}", 
            allParsers.Count, parsersToRun.Count, currentTime);

        if (parsersToRun.Count > 0)
        {
            foreach (var parser in parsersToRun)
            {
                bool activeParserJobs = parser.Jobs.Any(j => j.Status.IsActive());

                if (activeParserJobs)
                {
                    _logger.LogDebug("Skipping parser {ParserName} because it has active jobs.", parser.ParserName);
                    continue;
                }

                var job = CreateJobInternal(parser, ParserRunType.FullSite);
                parser.Jobs.Add(job);
            }

            await dbContext.SaveChangesAsync(token);

            var jobs = dbContext.Runs.Where(x=>x.Status == RunStatus.Created).ToList();
            foreach (var job in jobs)
            {
                await TransitionJobState(job.Id, ParseJobTrigger.JobCreated, context: "Scheduled job created", token: token);
            }
        }
        else
        {
            _logger.LogDebug("No parsers ready to run. Parser NextRun times: {ParserNextRuns}",
                string.Join(", ", allParsers.Select(p => $"{p.ParserName}:{p.NextRun:O}")));
        }

        return parsersToRun.Count;
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
        await TransitionJobState(job.Id, ParseJobTrigger.JobCreated, context: "Single job created", token: token);
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
        await TransitionJobState(job.Id, ParseJobTrigger.JobCreated, context: "Parser job created", token: token);
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
                .Where(r => r.Status == RunStatus.Waiting || r.Status == RunStatus.Running || r.Status == RunStatus.GatheringVolumes);

            foreach (var job in notFinishedProperly)
            {
                await TransitionJobState(job.Id, ParseJobTrigger.JobFailed, context: "Was automatically cancelled after restart", token: default);
            }
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

        return jobs.Select(j => j.Id);
    }

    private ParserJob CreateJobInternal(ParserModel parser, ParserRunType parserRunType, string? url = null)
    {
        return new ParserJob()
        {
            Created = DateTimeOffset.Now,
            Progress = 0,
            Status = RunStatus.Created,
            Type = parserRunType,
            Url = url,
            ParserStatusId = parser.Id,
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
        await TransitionJobState(runId, ParseJobTrigger.UpdateProgress, context: progress.ToString(), result: result, token: token);
    }

    public async Task SetToFinishedStatus(Guid jobId, CancellationToken token = default)
    {
        await TransitionJobState(jobId, ParseJobTrigger.Complete, context: "Job parsing completed successfully", token: token);
    }

    public async Task SetToErrorStatus(Guid jobId, CancellationToken token = default)
    {
        await TransitionJobState(jobId, ParseJobTrigger.JobFailed, context: "Job encountered an error", token: token);
    }

    public async Task SetToCancelledStatus(Guid jobId, CancellationToken token)
    {
        await TransitionJobState(jobId, ParseJobTrigger.CancelJob, context: "Job was cancelled", token: token);
    }

    public async Task SetToParsingStatus(Guid jobId, IEnumerable<string> volumesToParse, CancellationToken token = default)
    {
        var volumeCount = volumesToParse.Count();
        var context = $"Starting to parse {volumeCount} volumes";
        await TransitionJobState(jobId, ParseJobTrigger.BeginParsing, context: context, token: token);
    }

    public async Task RunJob(Guid jobId, CancellationToken token = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var job = await dbContext.Runs.FindAsync(jobId);

        if (job == null || job.Type == ParserRunType.SingleUrl)
        {
            await TransitionJobState(jobId, ParseJobTrigger.SkipGathering, context: "SingleUrl job - skipping volume gathering", token: token);
        }
        else
        {
            await TransitionJobState(jobId, ParseJobTrigger.StartGathering, context: "Starting volume gathering phase", token: token);
        }
    }

    /// <summary>
    /// Transitions a job to a new state using the state machine and publishes the transition event.
    /// This is the central method for all state transitions.
    /// </summary>
    /// <param name="jobId">The job ID to transition</param>
    /// <param name="trigger">The trigger to fire</param>
    /// <param name="context">Optional context information about the transition</param>
    /// <param name="exceptionDetails">Optional exception details if transitioning to error</param>
    /// <param name="token">Cancellation token</param>
    private async Task TransitionJobState(
        Guid jobId,
        ParseJobTrigger trigger,
        string? context = null,
        string? exceptionDetails = null,
        ParseResult? result = null,
        CancellationToken token = default)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var job = await dbContext.Runs
            .Include(r => r.ParserStatus)
            .FirstOrDefaultAsync(r => r.Id == jobId, token);

        if (job == null)
        {
            throw new InvalidOperationException($"Job {jobId} not found");
        }

        // Create state machine with current job state
        var stateMachine = new ParseJobStateMachine(job.Status);

        // Validate the trigger can be fired
        if (!stateMachine.CanFire(trigger))
        {
            throw new InvalidOperationException(
                $"Cannot fire trigger {trigger} on job {jobId} in state {job.Status}");
        }

        var previousState = job.Status;

        try
        {
            // Fire the trigger to transition state
            stateMachine.FireTrigger(trigger, context, exceptionDetails);

            // Publish the transition event (handlers will persist it and update the job)
            var transition = new JobStateTransition
            {
                JobId = jobId,
                FromState = previousState,
                ToState = stateMachine.CurrentState,
                Trigger = trigger.ToString(),
                TransitionTime = DateTimeOffset.UtcNow,
                Context = context,
                ExceptionDetails = exceptionDetails,
                Result = result
            };

            await _stateTransitionPublisher.PublishAsync(transition, token);

            _logger.LogInformation(
                "Job {JobId} transitioned from {FromState} to {ToState} via {Trigger}",
                jobId, previousState, stateMachine.CurrentState, trigger);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to transition job {JobId} via trigger {Trigger} from state {CurrentState}",
                jobId, trigger, job.Status);
            throw;
        }
    }
}