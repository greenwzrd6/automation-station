namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Tracks message identifiers that have already been handled, so redelivered messages can be skipped.
    /// </summary>
    /// <remarks>Used by the worker's message handler to provide at-least-once processing with idempotent handling:
    /// a message is checked before it is processed and marked as processed afterwards.</remarks>
    public interface IProcessedMessageRepository
    {
        /// <summary>
        /// Determines whether the message with the given identifier has already been marked as processed.
        /// </summary>
        /// <param name="messageId">Identifier of the message to look up.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns><see langword="true"/> if the message was processed earlier; otherwise <see langword="false"/>.</returns>
        Task<bool> HasProcessedAsync(
            Guid messageId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Records the message with the given identifier as processed so it will be skipped on redelivery.
        /// </summary>
        /// <param name="messageId">Identifier of the message to mark as processed.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>A task that completes when the message has been recorded.</returns>
        Task MarkProcessedAsync(
            Guid messageId,
            CancellationToken cancellationToken);
    }
}
