namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Provides persistence for automation execution records that deduplicate automation runs per triggering event.
    /// </summary>
    /// <remarks>Implementations must guarantee that concurrent calls with the same automation and event identifiers
    /// return a single shared execution identifier instead of creating duplicate executions.</remarks>
    public interface IAutomationExecutionRepository
    {
        /// <summary>
        /// Gets the identifier of the execution for the given automation and event, creating the execution record if it
        /// does not exist yet.
        /// </summary>
        /// <remarks>Calling this method more than once with the same <paramref name="automationId"/> and
        /// <paramref name="eventId"/> returns the same execution identifier, which makes redelivered or retried events
        /// idempotent. A different event, or a different automation, yields a new execution identifier.</remarks>
        /// <param name="automationId">Identifier of the automation being executed.</param>
        /// <param name="eventId">Identifier of the event that triggered the automation.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The identifier of the existing or newly created automation execution.</returns>
        Task<Guid> GetOrCreateAsync(
            Guid automationId,
            Guid eventId,
            CancellationToken cancellationToken);
    }
}