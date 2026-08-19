using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing.Handlers;

public class ParserStateHandler : IJobStateTransitionHandler
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<ParserStateHandler> _logger;
    private readonly IConfigurationService _configurationService;

    public ParserStateHandler(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        ILogger<ParserStateHandler> logger,
        IConfigurationService configurationService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        if(transition.Trigger == nameof(ParseJobTrigger.UpdateProgress))
        {
            return;
        }

        using var context = _dbContextFactory.CreateDbContext();

        var job = await context.Runs
            .Include(x=>x.ParserStatus)
            .SingleOrDefaultAsync(x => transition.JobId == x.Id);

        if(job == null)
        {
            throw new InvalidOperationException("Invalid job Id: " + transition.JobId);
        }

        var parser = job.ParserStatus;

        parser.Status = MapRunStatusToParserStatus(transition.ToState);

        if(transition.ToState.IsActive() && parser.NextRun != default && job.Type == ParserRunType.FullSite)
        {
            parser.NextRun = default;
        }

        if (transition.ToState.IsCompleted() && job.Type == ParserRunType.FullSite)
        {
            parser.NextRun = DateTimeOffset.UtcNow + _configurationService.JobManager.DelayBetweenRuns;
        }
        await context.SaveChangesAsync(cancellationToken);
    }


    /// <summary>
    /// Maps a RunStatus to the corresponding ParserStatus.
    /// RunStatus.Finished maps to ParserStatus.Idle since the parser is no longer active.
    /// </summary>
    private static ParserStatus MapRunStatusToParserStatus(RunStatus runStatus)
    {
        return runStatus switch
        {
            RunStatus.Waiting => ParserStatus.Waiting,
            RunStatus.GatheringVolumes => ParserStatus.GatheringVolumes,
            RunStatus.Running => ParserStatus.Parsing,
            RunStatus.Finished => ParserStatus.Idle,
            RunStatus.Error => ParserStatus.Idle,
            RunStatus.Cancelled => ParserStatus.Idle,
            _ => ParserStatus.Idle
        };
    }
}
