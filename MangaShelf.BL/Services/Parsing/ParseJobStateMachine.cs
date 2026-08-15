using MangaShelf.DAL.System.Models;
using Stateless;

namespace MangaShelf.BL.Services.Parsing;

/// <summary>
/// Defines the triggers/actions that cause state transitions in the parsing job state machine.
/// </summary>
public enum ParseJobTrigger
{
    /// <summary>Trigger to move from Created to Waiting state.</summary>
    JobCreated,
    /// <summary>Trigger to move from Waiting to GatheringVolumes</summary>
    StartGathering,

    /// <summary>Trigger to move from GatheringVolumes to Running</summary>
    BeginParsing,

    /// <summary>Trigger to move from Running to Finished</summary>
    Complete,

    /// <summary>Trigger to transition to Error state from any active state</summary>
    JobFailed,

    /// <summary>Trigger to transition to Cancelled state from any active state</summary>
    CancelJob,

    UpdateProgress
}

/// <summary>
/// State machine for parsing jobs.
/// Manages valid state transitions and enforces business rules.
/// 
/// Valid transitions:
/// - Waiting → GatheringVolumes (via StartGathering)
/// - GatheringVolumes → Running (via BeginParsing)
/// - Running → Finished (via Complete)
/// - {Any Active State} → Error (via RecordError)
/// - {Any Active State} → Cancelled (via CancelJob)
/// </summary>
public class ParseJobStateMachine
{
    private readonly StateMachine<RunStatus, ParseJobTrigger> _machine;
    private RunStatus _currentState;

    /// <summary>
    /// Raised when a state transition occurs.
    /// Subscribers can react to state changes (e.g., persist to database, send notifications).
    /// </summary>
    public event EventHandler<JobStateTransitionEventArgs>? OnStateTransition;

    /// <summary>
    /// Gets the current state of the state machine.
    /// </summary>
    public RunStatus CurrentState => _currentState;

    public ParseJobStateMachine(RunStatus initialState = RunStatus.Waiting)
    {
        _currentState = initialState;
        _machine = new StateMachine<RunStatus, ParseJobTrigger>(() => _currentState, s => _currentState = s);

        ConfigureTransitions();
    }

    private void ConfigureTransitions()
    {
        _machine.Configure(RunStatus.Created)
            .Permit(ParseJobTrigger.JobCreated, RunStatus.Waiting); // Transition to Waiting when Created trigger is fired

        // ========================================
        // WAITING STATE (Initial State)
        // ========================================
        // Represents a job that has been created but not yet started.
        // Valid transitions:
        //   - StartGathering → GatheringVolumes (when job runner picks up the job)
        //   - CancelJob → Cancelled (if user cancels before start)
        _machine.Configure(RunStatus.Waiting)
            .Permit(ParseJobTrigger.StartGathering, RunStatus.GatheringVolumes)
            .Permit(ParseJobTrigger.CancelJob, RunStatus.Cancelled)
            .Permit(ParseJobTrigger.JobFailed, RunStatus.Error);

        // ========================================
        // GATHERING VOLUMES STATE
        // ========================================
        // Represents the phase where the parser discovers volumes from the website.
        // Valid transitions:
        //   - BeginParsing → Running (once volume list is obtained)
        //   - JobFailed → Error (if gathering fails)
        //   - CancelJob → Cancelled (if user cancels during gathering)
        _machine.Configure(RunStatus.GatheringVolumes)
            .Permit(ParseJobTrigger.BeginParsing, RunStatus.Running)
            .Permit(ParseJobTrigger.JobFailed, RunStatus.Error)
            .Permit(ParseJobTrigger.CancelJob, RunStatus.Cancelled);

        // ========================================
        // RUNNING STATE (Main Parsing Phase)
        // ========================================
        // Represents the phase where individual volumes are being parsed.
        // Valid transitions:
        //   - Complete → Finished (when all volumes parsed successfully)
        //   - JobFailed → Error (if parsing encounters an error)
        //   - CancelJob → Cancelled (if user cancels during parsing)
        _machine.Configure(RunStatus.Running)
            .InternalTransition(ParseJobTrigger.UpdateProgress, (e) =>
            {
            })
            .Permit(ParseJobTrigger.Complete, RunStatus.Finished)
            .Permit(ParseJobTrigger.JobFailed, RunStatus.Error)
            .Permit(ParseJobTrigger.CancelJob, RunStatus.Cancelled);

        // ========================================
        // FINISHED STATE (Terminal - Success)
        // ========================================
        // Represents successful completion of the job.
        // Job is done - no transitions allowed from this state.
        // The job runner will update parser.NextRun for scheduling.
        _machine.Configure(RunStatus.Finished);

        // ========================================
        // ERROR STATE (Terminal - Failure)
        // ========================================
        // Represents the job failed due to an exception or parsing error.
        // Job is done - no transitions allowed from this state.
        // Error details are recorded in the ParserJob.Errors collection.
        _machine.Configure(RunStatus.Error);

        // ========================================
        // CANCELLED STATE (Terminal - Cancelled)
        // ========================================
        // Represents the job was cancelled by the user or system.
        // Job is done - no transitions allowed from this state.
        _machine.Configure(RunStatus.Cancelled);
    }

    /// <summary>
    /// Attempts to fire a trigger to transition to a new state.
    /// Throws InvalidOperationException if the transition is not valid.
    /// </summary>
    /// <param name="trigger">The trigger to fire</param>
    /// <param name="context">Optional contextual information about the transition</param>
    /// <param name="exceptionDetails">Optional exception details if transitioning to error state</param>
    /// <exception cref="InvalidOperationException">Thrown when the transition is not allowed from the current state</exception>
    public void FireTrigger(ParseJobTrigger trigger, string? context = null, string? exceptionDetails = null)
    {
        var previousState = _currentState;

        try
        {
            _machine.Fire(trigger);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Cannot transition from {previousState} via trigger {trigger}. {ex.Message}",
                ex);
        }

        RaiseStateTransition(previousState, _currentState, trigger, context, exceptionDetails);
    }

    /// <summary>
    /// Determines whether a trigger can be fired from the current state.
    /// </summary>
    /// <param name="trigger">The trigger to check</param>
    /// <returns>True if the trigger can be fired, false otherwise</returns>
    public bool CanFire(ParseJobTrigger trigger)
    {
        return _machine.CanFire(trigger);
    }

    private void RaiseStateTransition(RunStatus fromState, RunStatus toState, ParseJobTrigger trigger, string? context, string? exceptionDetails)
    {
        OnStateTransition?.Invoke(this, new JobStateTransitionEventArgs
        {
            FromState = fromState,
            ToState = toState,
            Trigger = trigger,
            Context = context,
            ExceptionDetails = exceptionDetails,
            TransitionTime = DateTimeOffset.UtcNow
        });
    }
}

/// <summary>
/// Event arguments raised when a state transition occurs.
/// </summary>
public class JobStateTransitionEventArgs : EventArgs
{
    public required RunStatus FromState { get; set; }
    public required RunStatus ToState { get; set; }
    public required ParseJobTrigger Trigger { get; set; }
    public required DateTimeOffset TransitionTime { get; set; }
    public string? Context { get; set; }
    public string? ExceptionDetails { get; set; }
}
