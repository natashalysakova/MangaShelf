using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing.Handlers;

/// <summary>
/// Handler that performs special processing when a job transitions to Error state.
/// Records error details and ensures the parser status is updated to Idle.
/// </summary>
public class HandleJobErrorHandler : IJobStateTransitionHandler
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<HandleJobErrorHandler> _logger;

    public HandleJobErrorHandler(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        ILogger<HandleJobErrorHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        // Only handle transitions to Error state
        if (transition.ToState != RunStatus.Error)
        {
            return;
        }

        try
        {
            using var context = _dbContextFactory.CreateDbContext();

            var job = await context.Runs
                .Include(r => r.ParserStatus)
                .FirstOrDefaultAsync(r => r.Id == transition.JobId, cancellationToken);

            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found when handling error state", transition.JobId);
                return;
            }

            // Set progress to -1 to indicate error
            job.Progress = -1;

            // Record error message from context if available
            if (!string.IsNullOrEmpty(transition.Context))
            {
                job.Errors.Add(new ParserError
                {
                    ErrorMessage = transition.Context,
                    RunTime = transition.TransitionTime
                });
            }

            // If exception details are available, add them as a separate error record
            if (!string.IsNullOrEmpty(transition.ExceptionDetails))
            {
                job.Errors.Add(new ParserError
                {
                    ErrorMessage = $"Exception: {transition.ExceptionDetails}",
                    RunTime = transition.TransitionTime
                });
            }

            // Update parser status to Idle
            if (job.ParserStatus != null)
            {
                job.ParserStatus.Status = ParserStatus.Idle;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Job {JobId} error state processed. Error details stored.", transition.JobId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle error state for job {JobId}", transition.JobId);
            // Don't rethrow - error handling failure shouldn't break other handlers
        }
    }
}
