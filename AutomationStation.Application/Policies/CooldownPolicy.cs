using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Policies
{
    public sealed class CooldownPolicy(
        IHistoryRepository historyRepository)
        : IAutomationExecutionPolicy
    {
        private static readonly TimeSpan Cooldown =
            TimeSpan.FromSeconds(3);

        private readonly IHistoryRepository _historyRepository = historyRepository;

        public async Task<AutomationPolicyResult> EvaluateAsync(
            Automation automation,
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            var since = DateTime.UtcNow - Cooldown;

            var triggeredRecently =
                await _historyRepository.HasTriggeredRecentlyAsync(
                    automation.Id,
                    since,
                    cancellationToken);

            return triggeredRecently
                ? AutomationPolicyResult.Block("Cooldown")
                : AutomationPolicyResult.Allow();
        }
    }
}
