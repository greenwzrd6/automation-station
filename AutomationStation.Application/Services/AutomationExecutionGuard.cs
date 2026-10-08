using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
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
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            foreach (var policy in _policies)
            {
                var result = await policy.EvaluateAsync(
                    context,
                    cancellationToken);

                if (result.Allowed)
                    continue;

                _logger.LogWarning(
                    "Automation '{AutomationName}' with ID: '{AutomationId}' was blocked by '{Policy}'. Reason: '{Reason}'",
                    context.AutomationName,
                    context.AutomationId,
                    policy.GetType().Name,
                    result.Reason);

                return true;
            }

            return false;
        }
    }
}
