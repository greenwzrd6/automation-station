using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Decides whether an automation may run for a given event evaluation.
    /// </summary>
    /// <remarks>Implementations encapsulate company- or system-specific execution rules. The policy is consulted
    /// before an automation's actions execute; it can approve the run or return a result describing why execution was
    /// denied or deferred.</remarks>
    public interface IAutomationExecutionPolicy
    {
        /// <summary>
        /// Evaluates the execution policy for the given evaluation context asynchronously.
        /// </summary>
        /// <param name="context">The context describing the event, automation, and company being evaluated.</param>
        /// <param name="cancellationToken">A token used to cancel the evaluation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the policy outcome,
        /// indicating whether the automation may execute and, if not, the reason.</returns>
        Task<AutomationPolicyResult> EvaluateAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}
