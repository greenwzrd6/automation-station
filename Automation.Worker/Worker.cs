using Automation.Application.Contracts;
using Automation.Application.Events;

namespace Automation.Worker;

public sealed class Worker(
    ProcessPlacementCreatedEventHandler handler,
    ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Automation Worker started.");

        var integrationEvent =
            new IntegrationEvent<PlacementCreatedPayload>(
                EventId: Guid.NewGuid().ToString(),
                EventType: "PlacementCreated",
                Source: "Planning",
                Payload: new PlacementCreatedPayload(
                    EntityId: Guid.NewGuid(),
                    ColumnId: Guid.Parse(
                        "22222222-2222-2222-2222-222222222224")
                )
            );

        await handler.HandleAsync(
            integrationEvent,
            stoppingToken);

        logger.LogInformation(
            "Automation Worker finished processing.");
    }
}