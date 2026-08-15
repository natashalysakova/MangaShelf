using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using Microsoft.Extensions.Logging;

namespace MangaShelf.BL.Services.Parsing;

/// <summary>
/// Publisher for job state transition events.
/// Allows handlers to subscribe to state changes and react decoupled.
/// </summary>
public class JobStateTransitionPublisher : IJobStateTransitionPublisher
{
    private readonly List<IJobStateTransitionHandler> _handlers = new();
    private readonly ILogger<JobStateTransitionPublisher> _logger;

    public JobStateTransitionPublisher(ILogger<JobStateTransitionPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a handler to be notified of state transitions.
    /// </summary>
    /// <param name="handler">The handler to register</param>
    public void Subscribe(IJobStateTransitionHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (!_handlers.Contains(handler))
        {
            _handlers.Add(handler);
            _logger.LogDebug("Handler {HandlerType} subscribed to state transitions", handler.GetType().Name);
        }
    }

    /// <summary>
    /// Unregisters a handler from state transition notifications.
    /// </summary>
    /// <param name="handler">The handler to unregister</param>
    public void Unsubscribe(IJobStateTransitionHandler handler)
    {
        if (_handlers.Remove(handler))
        {
            _logger.LogDebug("Handler {HandlerType} unsubscribed from state transitions", handler.GetType().Name);
        }
    }

    /// <summary>
    /// Publishes a state transition event to all registered handlers.
    /// Handlers are called sequentially; exceptions are logged but don't stop other handlers.
    /// </summary>
    /// <param name="transition">The transition to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PublishAsync(JobStateTransition transition, CancellationToken cancellationToken = default)
    {
        if (transition == null)
        {
            throw new ArgumentNullException(nameof(transition));
        }

        _logger.LogDebug("Publishing state transition: Job {JobId} {FromState} → {ToState} via {Trigger}",
            transition.JobId, transition.FromState, transition.ToState, transition.Trigger);

        foreach (var handler in _handlers)
        {
            try
            {
                _logger.LogDebug("Invoking handler {HandlerType} for job {JobId}", 
                    handler.GetType().Name, transition.JobId);

                await handler.HandleAsync(transition, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Handler {HandlerType} was cancelled", handler.GetType().Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in handler {HandlerType} while processing transition for job {JobId}",
                    handler.GetType().Name, transition.JobId);
                // Don't rethrow - allow other handlers to execute
            }
        }
    }

    /// <summary>
    /// Gets the number of registered handlers.
    /// Useful for testing and diagnostics.
    /// </summary>
    /// <returns>The count of active handlers</returns>
    public int GetHandlerCount()
    {
        return _handlers.Count;
    }
}
