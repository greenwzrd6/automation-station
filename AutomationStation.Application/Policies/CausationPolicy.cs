using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Policies
{
    public sealed class CausationPolicy(IHistoryRepository historyRepository)
        : IAutomationExecutionPolicy
    {
        private readonly IHistoryRepository _historyRepository = historyRepository;

        public async Task<AutomationPolicyResult> EvaluateAsync(
            Automation automation,
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            var causationEventId =
                integrationEvent.CausationEventId
                ?? integrationEvent.EventId;

            var exists =
                await _historyRepository.EventAlreadyProcessedAsync(
                    automation.Id,
                    causationEventId,
                    cancellationToken);

            return exists
                ? AutomationPolicyResult.Block(
                    "Causation event already processed.")
                : AutomationPolicyResult.Allow();
        }
    }
}