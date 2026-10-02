using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Services
{
    public sealed class AutomationProcessor(
        IAutomationRepository automationRepository,
        IConditionEvaluator conditionEvaluator,
        IAutomationExecutionGuard executionGuard,
        IHistoryRepository historyRepository,
        IActionExecutor actionExecutor)
        : IAutomationProcessor
    {
        private readonly IAutomationRepository _automationRepository = automationRepository;
        private readonly IConditionEvaluator _conditionEvaluator = conditionEvaluator;
        private readonly IAutomationExecutionGuard _executionGuard = executionGuard;
        private readonly IHistoryRepository _historyRepository = historyRepository;
        private readonly IActionExecutor _actionExecutor = actionExecutor;

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

                var causationEventId = integrationEvent.CausationEventId ?? integrationEvent.EventId;

                var context = new AutomationActionContext(
                    integrationEvent,
                    causationEventId);

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

            if (!string.IsNullOrWhiteSpace(when.EventSource) &&
                !string.Equals(
                    when.EventSource,
                    integrationEvent.Source,
                    StringComparison.OrdinalIgnoreCase))
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
