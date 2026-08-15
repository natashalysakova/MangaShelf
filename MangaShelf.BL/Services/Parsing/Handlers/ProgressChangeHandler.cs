using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.System;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing.Handlers;

public class ProgressChangeHandler : IJobStateTransitionHandler
{
    private readonly IDbContextFactory<MangaSystemDbContext> _dbContextFactory;
    private readonly ILogger<ProgressChangeHandler> _logger;

    public ProgressChangeHandler(
        IDbContextFactory<MangaSystemDbContext> dbContextFactory,
        ILogger<ProgressChangeHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var job = await context.Runs
            .Include(r => r.ParserStatus)
            .FirstOrDefaultAsync(r => r.Id == transition.JobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Job {JobId} not found when handling progress change", transition.JobId);
            return;
        }

        if (transition.ToState.NotSuccessful())
        {
            job.Progress = -1;
        }
        else if (double.TryParse(transition.Context, out var progress))
        {
            job.Progress = progress;
        }
        else
        {
            _logger.LogWarning("Invalid progress value '{Progress}' for job {JobId}", transition.Context, transition.JobId);
        }

        if (transition.Result != null)
        {
            var result = transition.Result;

            var volumeReference = new VolumeReference
            {
                VolumeId = result.VolumeReference.VolumeId,
                FullName = result.VolumeReference.FullName,
                PublicId = result.VolumeReference.PublicId,
            };

            if (result.State == State.Added)
            {
                volumeReference.AddedParserJobId = transition.JobId;
                job.AddedVolumes.Add(volumeReference);
            }
            else if (result.State == State.Updated)
            {
                volumeReference.UpdatedParserJobId = transition.JobId;
                job.UpdatedVolumes.Add(volumeReference);
            }

        }

        await context.SaveChangesAsync(cancellationToken);
    }
}