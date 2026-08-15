namespace MangaShelf.DAL.System.Models;

/// <summary>
/// Audit trail record for job state transitions.
/// Every state change in the parsing job state machine is recorded here.
/// </summary>
public class JobStateHistory
{
    /// <summary>
    /// Unique identifier for this history record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The parsing job this transition belongs to.
    /// Foreign key reference to ParserJob.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// The state before this transition.
    /// </summary>
    public RunStatus FromState { get; set; }

    /// <summary>
    /// The state after this transition.
    /// </summary>
    public RunStatus ToState { get; set; }

    /// <summary>
    /// The trigger/action that caused this transition.
    /// Examples: "StartGathering", "BeginParsing", "Complete", "CancelJob", "RecordError"
    /// </summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this transition occurred.
    /// </summary>
    public DateTimeOffset TransitionTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional contextual information about why this transition happened.
    /// Examples: volume counts, progress percentage, error messages.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Optional exception stack trace if this transition was due to an error state.
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// Navigation property to the job this history record belongs to.
    /// </summary>
    public virtual ParserJob? Job { get; set; }
}
