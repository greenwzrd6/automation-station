using Microsoft.Extensions.Logging;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Services
{
    /// <summary>
    /// Guards automation execution by evaluating a set of policies and reporting whether execution is blocked.
    /// </summary>
    /// <remarks>Policies are evaluated in registration order and the first policy that denies short-circuits the
    /// evaluation; any remaining policies are not consulted. The guard holds no state between calls and is safe to
    /// reuse across executions.</remarks>
    /// <param name="policies">The execution policies that must all allow an execution for it to proceed.</param>
    /// <param name="logger">Logger used to record which policy blocked an execution and the policy's stated reason.</param>
    public sealed class AutomationExecutionGuard(
        IEnumerable<IAutomationExecutionPolicy> policies,
        ILogger<AutomationExecutionGuard> logger)
        : IAutomationExecutionGuard
    {
        private readonly IEnumerable<IAutomationExecutionPolicy> _policies = policies;
        private readonly ILogger<AutomationExecutionGuard> _logger = logger;

        /// <summary>
        /// Determines whether any execution policy blocks the automation described by <paramref name="context"/>.
        /// </summary>
        /// <remarks>Evaluates each registered policy in order until one denies; the denying policy and its reason are
        /// logged as a warning before the method returns <see langword="true"/>.</remarks>
        /// <param name="context">The automation and the integration event that triggered it.</param>
        /// <param name="cancellationToken">A token used to cancel policy evaluation.</param>
        /// <returns><see langword="true"/> if a policy denied the execution; otherwise <see langword="false"/>.</returns>
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
