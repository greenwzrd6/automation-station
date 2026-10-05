using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using Microsoft.Extensions.Logging;

namespace AutomationStation.Application.Services
{
    public sealed class AutomationProcessor(
        IAutomationRepository automationRepository,
        IConditionEvaluator conditionEvaluator,
        IAutomationExecutionGuard executionGuard,
        IHistoryRepository historyRepository,
        IActionExecutor actionExecutor,
        IAutomationExecutionRepository automationExecutionRepository,
        ICorrelationLoopRepository correlationLoopRepository,
        ILogger<AutomationProcessor> logger)
        : IAutomationProcessor
    {
        private readonly IAutomationRepository _automationRepository = automationRepository;
        private readonly IConditionEvaluator _conditionEvaluator = conditionEvaluator;
        private readonly IAutomationExecutionGuard _executionGuard = executionGuard;
        private readonly IActionExecutor _actionExecutor = actionExecutor;
        private readonly IAutomationExecutionRepository _automationExecutionRepository = automationExecutionRepository;
        private readonly ICorrelationLoopRepository _correlationLoopRepository = correlationLoopRepository;
        private readonly ILogger<AutomationProcessor> _logger = logger;

        public async Task ProcessAsync(
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            var automations = await _automationRepository.GetEnabledByEventTypeAsync(
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

                if (await _executionGuard.IsBlockedAsync(
                        automation,
                        integrationEvent,
                        cancellationToken))
                {
                    continue;
                }

                if (await _correlationLoopRepository.IsBlockedAsync(
                    automation,
                    integrationEvent,
                    cancellationToken))
                {
                    _logger.LogWarning(
                    """
                    Automation '{AutomationName}' ({AutomationId}) blocked.
                    ActorId: {ActorId}, CorrelationId: {CorrelationId}, EventId: {EventId}.
                    Reason: actor reached the limit of 10 executions within 3 minutes.
                    """,
                    automation.Name,
                    automation.Id,
                    integrationEvent.Actor.Id,
                    integrationEvent.CorrelationId,
                    integrationEvent.EventId);

                    continue;
                }

                var executionId = await _automationExecutionRepository.GetOrCreateAsync(
                    automation.Id,
                    integrationEvent.EventId,
                    cancellationToken);

                var context = new AutomationActionContext(
                    integrationEvent,
                    integrationEvent.CorrelationId,
                    executionId);

                foreach (var then in automation.Thens)
                {
                    await _actionExecutor.ExecuteAsync(
                        then,
                        context,
                        cancellationToken);
                }
            }
        }

        private bool Matches(
            When when,
            IntegrationEvent integrationEvent)
        {
            if (!string.Equals(
                    when.EventType,
                    integrationEvent.EventType,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (when.EventSource != integrationEvent.Source)
            {
                return false;
            }

            return when.Conditions.All(
                condition =>
                    _conditionEvaluator.Matches(
                        condition,
                        integrationEvent));
        }
    }
}
