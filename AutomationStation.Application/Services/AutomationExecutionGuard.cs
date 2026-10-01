using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;
using Microsoft.Extensions.Logging;

namespace AutomationStation.Application.Services
{
    public sealed class AutomationExecutionGuard(
        IEnumerable<IAutomationExecutionPolicy> policies,
        ILogger<AutomationExecutionGuard> logger)
        : IAutomationExecutionGuard
    {
        private readonly IEnumerable<IAutomationExecutionPolicy> _policies = policies;
        private readonly ILogger<AutomationExecutionGuard> _logger = logger;

        public async Task<bool> IsBlockedAsync(
            Automation automation,
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            foreach (var policy in _policies)
            {
                var result = await policy.EvaluateAsync(
                    automation,
                    integrationEvent,
                    cancellationToken);

                if (result.Allowed)
                    continue;

                _logger.LogWarning(
                    "Automation '{AutomationId}' blocked by '{Policy}'. Reason: '{Reason}'",
                    automation.Id,
                    policy.GetType().Name,
                    result.Reason);

                return true;
            }

            return false;
        }
    }
}
