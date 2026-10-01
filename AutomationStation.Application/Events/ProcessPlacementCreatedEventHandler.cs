using Microsoft.Extensions.Logging;

using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Events;

public sealed class ProcessPlacementCreatedEventHandler(
    IAutomationRepository automationRepository,
    IHistoryRepository historyRepository,
    IActionExecutor actionExecutor,
    ILogger<ProcessPlacementCreatedEventHandler> logger)
{
    private const string PlacementCreated = "PlacementCreated";

    private readonly IAutomationRepository _automationRepository = automationRepository;
    private readonly IHistoryRepository _historyRepository = historyRepository;
    private readonly IActionExecutor _actionExecutor = actionExecutor;

    private static readonly TimeSpan ExecutionCooldown = TimeSpan.FromSeconds(3);

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

        var causationEventId = integrationEvent.CausationEventId ?? integrationEvent.EventId;

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

            if (string.Equals(
                    integrationEvent.Actor.Type,
                    //Denna ska användas när vi plockar rätt skit från toj ( tock och jolltortyr )
                    //"AutomationExecutor",
                    "",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (!automation.AutomationTriggerable)
                {
                    continue;
                }
            }

            var cooldown = DateTime.UtcNow - ExecutionCooldown;

            var triggeredRecently =
                await _historyRepository.TriggeredRecentlyAsync(
                    automation.Id,
                    cooldown,
                    cancellationToken);

            var eventAlreadyProcessed =
                await _historyRepository.EventAlreadyProcessedAsync(
                    automation.Id,
                    causationEventId,
                    cancellationToken);

            if (triggeredRecently || eventAlreadyProcessed)
            {
                logger.LogWarning(
                    "Event {EventId} with correlationId {CorrelationId} blocked for automation {AutomationId}. TriggeredRecently: {TriggeredRecently}, EventAlreadyProcessed: {HasCausationId}",
                    integrationEvent.EventId,
                    integrationEvent.CorrelationId,
                    automation.Id,
                    triggeredRecently,
                    eventAlreadyProcessed);

                continue;
            }

            var context = new PlacementActionContext(
                EntityId: integrationEvent.Payload.EntityId,
                CorrelationId: integrationEvent.CorrelationId,
                CausationEventId: causationEventId,
                Actor: integrationEvent.Actor,
                CompanyId: integrationEvent.CompanyId);

            await _historyRepository.CreateAutomationTimestampAsync(
                automation.Id,
                causationEventId,
                cancellationToken);

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