using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MangaShelf.BL.Services.Parsing.Handlers;

/// <summary>
/// Handler that updates the job's current status in the database when state transitions occur.
/// This ensures the Runs table always reflects the current state of each job.
/// </summary>
public class NotifyJobStatusChangedHandler : IJobStateTransitionHandler
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<NotifyJobStatusChangedHandler> _logger;

    public NotifyJobStatusChangedHandler(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        ILogger<NotifyJobStatusChangedHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = _dbContextFactory.CreateDbContext();

            var job = await context.Runs
                .Include(r => r.ParserStatus)
                .FirstOrDefaultAsync(r => r.Id == transition.JobId, cancellationToken);

            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found in database when updating status", transition.JobId);
                return;
            }

            // Update the job's current status
            job.Status = transition.ToState;

            // Set Started timestamp when transitioning from Waiting to the first active state (GatheringVolumes)
            if (transition.FromState == RunStatus.Waiting && transition.ToState == RunStatus.GatheringVolumes)
            {
                job.Started = transition.TransitionTime;
            }

            // Set Finished timestamp when transitioning to terminal states
            if (transition.ToState.IsCompleted())
            {
                job.Finished = transition.TransitionTime;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Updated job {JobId} status to {Status}", transition.JobId, transition.ToState);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job status for job {JobId}", transition.JobId);
            // Don't rethrow - status update failure shouldn't block other handlers
        }
    }
}
