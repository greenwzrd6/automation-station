using Automation.Application.Abstractions;
using Automation.Application.Contracts;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Application.Events;

public sealed class ProcessPlacementCreatedEventHandler
{
    private const string PlacementCreated = "PlacementCreated";
    private const string EntityPlacedInColumn =
        "EntityPlacedInColumn";

    private readonly IAutomationRepository _automationRepository;
    private readonly IActionExecutor _actionExecutor;

    public ProcessPlacementCreatedEventHandler(
        IAutomationRepository automationRepository,
        IActionExecutor actionExecutor)
    {
        _automationRepository = automationRepository;
        _actionExecutor = actionExecutor;
    }

    public async Task HandleAsync(
        IntegrationEvent<PlacementCreatedPayload> integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent.EventType != PlacementCreated)
        {
            return;
        }

        var automations =
            await _automationRepository.GetEnabledByEventTypeAsync(
                integrationEvent.EventType,
                cancellationToken);

        foreach (var automation in automations)
        {
            if (!Matches(
                    automation.When,
                    integrationEvent.Payload))
            {
                continue;
            }

            var context = new ActionContext(
                EntityId: integrationEvent.Payload.EntityId,
                CausationEventId: integrationEvent.EventId);

            foreach (var then in automation.Thens)
            {
                await _actionExecutor.ExecuteAsync(
                    then,
                    context,
                    cancellationToken);
            }
        }
    }

    private static bool Matches(
        When when,
        PlacementCreatedPayload payload)
    {
        if (when.Type != EntityPlacedInColumn)
        {
            return false;
        }

        return when.ColumnId == payload.ColumnId;
    }
}