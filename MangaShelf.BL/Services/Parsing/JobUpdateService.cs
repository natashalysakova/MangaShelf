using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.DAL.System.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing;

/// <summary>
/// Implements intelligent polling for job updates with change detection and adaptive polling intervals.
/// </summary>
public class JobUpdateService : IJobUpdateService
{
    private readonly IParserReadService _parserReadService;
    private readonly ILogger<JobUpdateService> _logger;

    private IEnumerable<ParserStatusDto>? _previousStatuses;
    private IEnumerable<ParserJob>? _previousJobs;

    // Polling interval constants (milliseconds)
    private const int FastIntervalMs = 500;      // For active parsing/running jobs
    private const int MediumIntervalMs = 1000;   // For waiting jobs
    private const int SlowIntervalMs = 2000;    // For all idle

    public event EventHandler<ParserStatusesChangedEventArgs>? StatusesChanged;
    public event EventHandler<ParserJobsChangedEventArgs>? JobsChanged;

    public IEnumerable<ParserStatusDto>? CurrentStatuses { get; private set; }
    public IEnumerable<ParserJob>? CurrentJobs { get; private set; }
    public int CurrentPollingIntervalMs { get; private set; }

    public JobUpdateService(IParserReadService parserReadService, ILogger<JobUpdateService> logger)
    {
        _parserReadService = parserReadService;
        _logger = logger;
        CurrentPollingIntervalMs = SlowIntervalMs;
    }

    public async Task StartPollingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Job update polling started");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Fetch current data
                    var newStatuses = await _parserReadService.GetStatusesAsync(cancellationToken);
                    var newJobs = await _parserReadService.GetJobs(20, cancellationToken);

                    // Check for status changes
                    if (HasStatusesChanged(newStatuses))
                    {
                        CurrentStatuses = newStatuses;
                        OnStatusesChanged(newStatuses);
                        _previousStatuses = newStatuses?.ToList();
                    }

                    // Check for jobs changes
                    var jobChangeType = DetectJobChanges(newJobs);
                    if (jobChangeType != JobChangeType.None)
                    {
                        CurrentJobs = newJobs;
                        OnJobsChanged(newJobs, jobChangeType);
                        _previousJobs = newJobs?.ToList();
                    }

                    // Adapt polling interval based on current activity
                    int newInterval = CalculatePollingInterval(CurrentStatuses);
                    if (newInterval != CurrentPollingIntervalMs)
                    {
                        CurrentPollingIntervalMs = newInterval;
                        _logger.LogInformation(
                            "Polling interval adjusted to {IntervalMs}ms based on parser activity",
                            newInterval);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Job update polling cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during job update polling");
                    // Continue polling even on errors
                }

                await Task.Delay(CurrentPollingIntervalMs, cancellationToken);
            }
        }
        finally
        {
            _logger.LogInformation("Job update polling stopped");
        }
    }

    private bool HasStatusesChanged(IEnumerable<ParserStatusDto>? newStatuses)
    {
        if (_previousStatuses == null && newStatuses == null)
            return false;

        if (_previousStatuses == null || newStatuses == null)
            return true;

        var newList = newStatuses.ToList();
        var previousList = _previousStatuses.ToList();

        if (newList.Count != previousList.Count)
            return true;

        // Compare each status by key properties
        foreach (var newStatus in newList)
        {
            var previousStatus = previousList.FirstOrDefault(s => s.Id == newStatus.Id);
            if (previousStatus == null)
                return true;

            if (previousStatus.Status != newStatus.Status ||
                previousStatus.IsActive != newStatus.IsActive ||
                previousStatus.Progress != newStatus.Progress)
            {
                _logger.LogDebug(
                    "Status changed for parser {ParserName}: {OldStatus} → {NewStatus}",
                    newStatus.ParserName, previousStatus.Status, newStatus.Status);
                return true;
            }
        }

        return false;
    }

    private JobChangeType DetectJobChanges(IEnumerable<ParserJob>? newJobs)
    {
        if (_previousJobs == null && newJobs == null)
            return JobChangeType.None;

        if (_previousJobs == null || newJobs == null)
        {
            _logger.LogDebug("Job collection changed: items added or removed");
            return JobChangeType.JobsAdded | JobChangeType.JobsRemoved;
        }

        var newList = newJobs.ToList();
        var previousList = _previousJobs.ToList();

        var changes = JobChangeType.None;

        // Check if counts differ
        if (newList.Count != previousList.Count)
        {
            _logger.LogDebug(
                "Job count changed: {PreviousCount} → {NewCount}",
                previousList.Count, newList.Count);
            changes |= newList.Count > previousList.Count ? JobChangeType.JobsAdded : JobChangeType.JobsRemoved;
        }

        // Compare existing jobs for changes
        foreach (var newJob in newList)
        {
            var previousJob = previousList.FirstOrDefault(j => j.Id == newJob.Id);
            if (previousJob == null)
            {
                changes |= JobChangeType.JobsAdded;
                _logger.LogDebug("New job detected: {JobId}", newJob.Id);
                continue;
            }

            // Check for status changes
            if (previousJob.Status != newJob.Status)
            {
                changes |= JobChangeType.StatusChanged;
                _logger.LogDebug(
                    "Job status changed {JobId}: {OldStatus} → {NewStatus}",
                    newJob.Id, previousJob.Status, newJob.Status);
            }

            // Check for progress changes
            if (Math.Abs(previousJob.Progress - newJob.Progress) > 0.01)
            {
                changes |= JobChangeType.ProgressChanged;
            }

            // Check for error changes
            if ((previousJob.Errors?.Count ?? 0) != (newJob.Errors?.Count ?? 0))
            {
                changes |= JobChangeType.ErrorsChanged;
                _logger.LogDebug(
                    "Job errors changed {JobId}: {OldErrorCount} → {NewErrorCount}",
                    newJob.Id, previousJob.Errors?.Count ?? 0, newJob.Errors?.Count ?? 0);
            }
        }

        return changes;
    }

    private int CalculatePollingInterval(IEnumerable<ParserStatusDto>? statuses)
    {
        if (statuses == null || !statuses.Any())
            return SlowIntervalMs;

        // Check if any parser is actively parsing/gathering/saving
        if (statuses.Any(s =>
            s.Status == ParserStatus.Parsing ||
            s.Status == ParserStatus.Saving ||
            s.Status == ParserStatus.GatheringVolumes))
        {
            return FastIntervalMs;
        }

        // Check if any parser is waiting
        if (statuses.Any(s => s.Status == ParserStatus.Waiting))
        {
            return MediumIntervalMs;
        }

        // All idle
        return SlowIntervalMs;
    }

    private void OnStatusesChanged(IEnumerable<ParserStatusDto> newStatuses)
    {
        _logger.LogDebug("Raising StatusesChanged event");
        StatusesChanged?.Invoke(this, new ParserStatusesChangedEventArgs
        {
            NewStatuses = newStatuses,
            PreviousStatuses = _previousStatuses
        });
    }

    private void OnJobsChanged(IEnumerable<ParserJob> newJobs, JobChangeType changeType)
    {
        _logger.LogDebug("Raising JobsChanged event with change type: {ChangeType}", changeType);
        JobsChanged?.Invoke(this, new ParserJobsChangedEventArgs
        {
            NewJobs = newJobs,
            PreviousJobs = _previousJobs,
            ChangeType = changeType
        });
    }
}
