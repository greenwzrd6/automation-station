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
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            var exists =
                await _historyRepository.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    cancellationToken);

            return exists
                ? AutomationPolicyResult.Block(
                    "Correlation event already processed.")
                : AutomationPolicyResult.Allow();
        }
    }
}