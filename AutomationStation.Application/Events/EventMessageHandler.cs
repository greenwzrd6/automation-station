using Microsoft.Extensions.Logging;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Events
{
    /// <summary>
    /// Handles incoming integration events with idempotent-at-least-once processing semantics.
    /// </summary>
    /// <remarks>Each event is processed at most once per recorded identifier: if the event id has been
    /// seen before, the event is skipped and reported as a duplicate. After the automation processor finishes
    /// successfully, the event id is recorded as processed so redeliveries are ignored. If processing fails,
    /// nothing is recorded and the exception propagates, allowing the transport to retry the message.</remarks>
    /// <param name="messageRepository">Store used to detect and record already-processed event identifiers.</param>
    /// <param name="automationProcessor">Processor that evaluates the event against enabled automations and
    /// runs matching actions.</param>
    /// <param name="logger">Logger used to report duplicate and successfully processed events.</param>
    public sealed class EventMessageHandler(
        IProcessedMessageRepository messageRepository,
        IAutomationProcessor automationProcessor,
        ILogger<EventMessageHandler> logger)
    {
        /// <summary>
        /// Processes an integration event unless its identifier has already been handled.
        /// </summary>
        /// <param name="integrationEvent">The event to handle; its <c>EventId</c> is the deduplication key.</param>
        /// <param name="cancellationToken">A token used to cancel processing while it is in flight.</param>
        /// <returns>A task whose result is <see cref="MessageOutcome.Processed"/> when the event was handled
        /// and recorded, or <see cref="MessageOutcome.Duplicate"/> when the event id was already processed.</returns>
        public async Task<MessageOutcome> HandleAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
        {
            if (await messageRepository.HasProcessedAsync(
                    integrationEvent.EventId,
                    cancellationToken))
            {
                logger.LogInformation(
                    "Ignoring duplicate message '{EventId}'",
                    integrationEvent.EventId);

                return MessageOutcome.Duplicate;
            }

            await automationProcessor.ProcessAsync(
                integrationEvent,
                cancellationToken);

            await messageRepository.MarkProcessedAsync(
                integrationEvent.EventId,
                cancellationToken);

            logger.LogInformation(
                "Successfully processed event '{EventType}'",
                integrationEvent.EventType);

            return MessageOutcome.Processed;
        }
    }
}
