using MangaShelf.BL.Dto;

namespace MangaShelf.BL.Contracts;

/// <summary>
/// Defines the contract for publishing job state transition events.
/// Implementations allow decoupled event handling (e.g., persist history, send notifications).
/// </summary>
public interface IJobStateTransitionPublisher
{
    /// <summary>
    /// Publishes a job state transition event to all registered subscribers.
    /// </summary>
    /// <param name="transition">The state transition that occurred</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    Task PublishAsync(JobStateTransition transition, CancellationToken cancellationToken = default);

    void Subscribe(IJobStateTransitionHandler handler);
    void Unsubscribe(IJobStateTransitionHandler handler);
}

/// <summary>
/// Handler interface for state transition events.
/// Implementations perform specific actions when jobs transition states
/// (e.g., save to database, update UI, send alerts).
/// </summary>
public interface IJobStateTransitionHandler
{
    /// <summary>
    /// Handles a state transition event.
    /// </summary>
    /// <param name="transition">The state transition that occurred</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous handling operation</returns>
    Task HandleAsync(JobStateTransition transition, CancellationToken cancellationToken = default);
}
