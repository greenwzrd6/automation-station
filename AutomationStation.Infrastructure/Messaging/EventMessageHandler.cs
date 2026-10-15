using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace AutomationStation.Infrastructure.Messaging
{
    public sealed class EventMessageHandler(
        IProcessedMessageRepository messageRepository,
        IAutomationProcessor automationProcessor,
        ILogger<EventMessageHandler> logger)
    {
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
