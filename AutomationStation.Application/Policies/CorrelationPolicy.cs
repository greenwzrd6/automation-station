using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Policies
{
    /// <summary>
    /// Execution policy that blocks an automation from running twice for the same correlation identifier.
    /// </summary>
    /// <remarks>Correlation identifiers tie a chain of causally related events together, so this policy ensures each
    /// automation fires at most once per chain: it consults <see cref="IHistoryRepository"/> for an earlier
    /// execution of the same automation within the same correlation and blocks the current run when one exists.</remarks>
    /// <param name="historyRepository">Store queried for previous automation executions.</param>
    public sealed class CorrelationPolicy(IHistoryRepository historyRepository)
        : IAutomationExecutionPolicy
    {
        private readonly IHistoryRepository _historyRepository = historyRepository;

        /// <summary>
        /// Evaluates whether the automation may run for the triggering event by checking its correlation history.
        /// </summary>
        /// <param name="context">The automation and event being evaluated; <c>AutomationId</c> and
        /// <c>CorrelationId</c> form the deduplication key.</param>
        /// <param name="cancellationToken">A token used to cancel the history lookup.</param>
        /// <returns>A task whose result is <see cref="AutomationPolicyResult.Allow"/> when the automation has not
        /// run in this correlation, otherwise <see cref="AutomationPolicyResult.Block"/> with a reason.</returns>
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
                    "Automation already executed in this correlation.")
                : AutomationPolicyResult.Allow();
        }
    }
}