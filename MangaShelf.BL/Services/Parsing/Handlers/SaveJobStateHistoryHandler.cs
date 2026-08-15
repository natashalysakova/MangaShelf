using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing.Handlers;

/// <summary>
/// Handler that persists state transition history to the database.
/// This ensures an audit trail of all state changes for each job.
/// Should be invoked first to ensure DB persistence before other side effects.
/// </summary>
public class SaveJobStateHistoryHandler : IJobStateTransitionHandler
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<SaveJobStateHistoryHandler> _logger;

    public SaveJobStateHistoryHandler(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        ILogger<SaveJobStateHistoryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = _dbContextFactory.CreateDbContext();

            var historyRecord = new JobStateHistory
            {
                JobId = transition.JobId,
                FromState = transition.FromState,
                ToState = transition.ToState,
                Trigger = transition.Trigger,
                TransitionTime = transition.TransitionTime,
                Context = transition.Context,
                ExceptionDetails = transition.ExceptionDetails
            };

            context.JobStateHistories.Add(historyRecord);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("State transition saved to history: Job {JobId} {FromState} → {ToState}",
                transition.JobId, transition.FromState, transition.ToState);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save state transition history for job {JobId}", transition.JobId);
            throw; // Rethrow to fail the transition if we can't persist it
        }
    }
}
