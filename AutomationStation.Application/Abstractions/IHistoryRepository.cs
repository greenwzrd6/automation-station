using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Persists automation execution history used for idempotency and audit purposes.
    /// </summary>
    /// <remarks>Implementations record when an automation was evaluated for an event and allow callers to detect
    /// events that have already been processed for a given automation, so the same integration event does not trigger
    /// an automation more than once.</remarks>
    public interface IHistoryRepository
    {
        /// <summary>
        /// Records a timestamp for the given automation evaluation asynchronously.
        /// </summary>
        /// <param name="context">The evaluation context identifying the automation, event, and company.</param>
        /// <param name="cancellationToken">A token used to cancel the write.</param>
        /// <returns>A task that completes when the timestamp has been stored.</returns>
        Task CreateAutomationTimestampAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether the given event has already been processed by the given automation asynchronously.
        /// </summary>
        /// <param name="AutomationId">Identifier of the automation whose history is checked.</param>
        /// <param name="CorrelationId">Correlation identifier of the event to look for in the history.</param>
        /// <param name="cancellationToken">A token used to cancel the lookup.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if a
        /// matching history record exists; otherwise, <see langword="false"/>.</returns>
        Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CorrelationId,
            CancellationToken cancellationToken);
    }
}