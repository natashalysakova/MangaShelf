using MangaShelf.BL.Dto;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.BL.Contracts;

/// <summary>
/// Service for managing intelligent polling of job updates with change detection and adaptive intervals.
/// </summary>
public interface IJobUpdateService
{
    /// <summary>
    /// Event raised when parser statuses change.
    /// </summary>
    event EventHandler<ParserStatusesChangedEventArgs>? StatusesChanged;

    /// <summary>
    /// Event raised when parser jobs change.
    /// </summary>
    event EventHandler<ParserJobsChangedEventArgs>? JobsChanged;

    /// <summary>
    /// Starts polling for job updates with change detection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop polling.</param>
    Task StartPollingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current parser statuses.
    /// </summary>
    IEnumerable<ParserStatusDto>? CurrentStatuses { get; }

    /// <summary>
    /// Gets the current parser jobs.
    /// </summary>
    IEnumerable<ParserJob>? CurrentJobs { get; }

    /// <summary>
    /// Gets the current polling interval in milliseconds.
    /// </summary>
    int CurrentPollingIntervalMs { get; }
}

/// <summary>
/// Event arguments for status changes.
/// </summary>
public class ParserStatusesChangedEventArgs : EventArgs
{
    public IEnumerable<ParserStatusDto> NewStatuses { get; set; } = null!;
    public IEnumerable<ParserStatusDto>? PreviousStatuses { get; set; }
}

/// <summary>
/// Event arguments for job changes.
/// </summary>
public class ParserJobsChangedEventArgs : EventArgs
{
    public IEnumerable<ParserJob> NewJobs { get; set; } = null!;
    public IEnumerable<ParserJob>? PreviousJobs { get; set; }
    /// <summary>
    /// Indicates what types of changes were detected.
    /// </summary>
    public JobChangeType ChangeType { get; set; }
}

/// <summary>
/// Types of changes that can occur in job data.
/// </summary>
[Flags]
public enum JobChangeType
{
    None = 0,
    StatusChanged = 1,
    ProgressChanged = 2,
    JobsAdded = 4,
    JobsRemoved = 8,
    ErrorsChanged = 16
}
