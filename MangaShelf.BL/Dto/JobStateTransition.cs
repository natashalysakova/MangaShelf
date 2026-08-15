using MangaShelf.BL.Contracts;
using MangaShelf.DAL.System.Models;

namespace MangaShelf.BL.Dto;

/// <summary>
/// Represents a state transition event for a parsing job.
/// Published when a job's state changes via the state machine.
/// </summary>
public class JobStateTransition
{
    /// <summary>
    /// Unique identifier for the parsing job.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// The state the job was in before the transition.
    /// </summary>
    public RunStatus FromState { get; set; }

    /// <summary>
    /// The state the job transitioned to.
    /// </summary>
    public RunStatus ToState { get; set; }

    /// <summary>
    /// The trigger that caused this transition (e.g., "StartGathering", "BeginParsing", "Complete").
    /// </summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the transition occurred.
    /// </summary>
    public DateTimeOffset TransitionTime { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Optional contextual information about the transition (e.g., error message, progress).
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Optional exception details if the transition was due to an error.
    /// </summary>
    public string? ExceptionDetails { get; set; }

    public ParseResult? Result { get; set; }
}
