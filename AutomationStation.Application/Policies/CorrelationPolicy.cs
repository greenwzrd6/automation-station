using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Policies
{
    public sealed class CorrelationPolicy(IHistoryRepository historyRepository)
        : IAutomationExecutionPolicy
    {
        private readonly IHistoryRepository _historyRepository = historyRepository;

        public async Task<AutomationPolicyResult> EvaluateAsync(
            LoopBlockerContext context,
            CancellationToken cancellationToken)
        {
            var correlationId = context.Event.CorrelationId;

            var exists =
                await _historyRepository.EventAlreadyProcessedAsync(
                    context.Automation.Id,
                    correlationId,
                    cancellationToken);

            return exists
                ? AutomationPolicyResult.Block(
                    "Correlation event already processed.")
                : AutomationPolicyResult.Allow();
        }
    }
}