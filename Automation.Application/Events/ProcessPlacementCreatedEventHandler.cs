using Automation.Application.Abstractions;
using Automation.Application.Contracts;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Application.Events;

public sealed class ProcessPlacementCreatedEventHandler(
    IAutomationRepository automationRepository,
    IActionExecutor actionExecutor)
{
    private const string PlacementCreated = "PlacementCreated";

    private readonly IAutomationRepository _automationRepository = automationRepository;
    private readonly IActionExecutor _actionExecutor = actionExecutor;

    public async Task HandleAsync(
        IntegrationEvent<PlacementCreatedPayload> integrationEvent,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                integrationEvent.EventType,
                PlacementCreated,
                StringComparison.OrdinalIgnoreCase))
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
                    integrationEvent))
            {
                continue;
            }

            var context = new PlacementActionContext(
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
        IntegrationEvent<PlacementCreatedPayload> integrationEvent)
    {
        if (!string.Equals(
                when.EventType,
                integrationEvent.EventType,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(
                when.EventSource,
                integrationEvent.Source,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return when.Conditions.All(
            condition => MatchesCondition(
                condition,
                integrationEvent.Payload));
    }

    private static bool MatchesCondition(
        Condition condition,
        PlacementCreatedPayload payload)
    {
        return condition.Field.ToLowerInvariant() switch
        {
            "columnid" => MatchesGuidCondition(
                payload.ColumnId,
                condition),

            "entityid" => MatchesGuidCondition(
                payload.EntityId,
                condition),

            _ => false
        };
    }

    private static bool MatchesGuidCondition(
        Guid actualValue,
        Condition condition)
    {
        if (!Guid.TryParse(
                condition.Value,
                out var expectedValue))
        {
            return false;
        }

        return condition.Operator switch
        {
            ConditionOperator.Equals =>
                actualValue == expectedValue,

            ConditionOperator.NotEquals =>
                actualValue != expectedValue,

            _ => false
        };
    }
}