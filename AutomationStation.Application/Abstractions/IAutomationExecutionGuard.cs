using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Decides whether an automation's execution should be suppressed for a given event.
    /// </summary>
    /// <remarks>Implementations inspect the <see cref="EvaluationContext"/>, which pairs the matched automation with
    /// the integration event that triggered it, and return <see langword="true"/> when the automation must not run,
    /// for example because an execution policy or automation-triggerable rule forbids it. The processor skips a blocked
    /// automation without recording an execution or dispatching any of its actions.</remarks>
    public interface IAutomationExecutionGuard
    {
        /// <summary>
        /// Determines whether the automation represented by the context is blocked from executing.
        /// </summary>
        /// <param name="context">The matched automation paired with the integration event that triggered it.</param>
        /// <param name="cancellationToken">A token used to cancel the underlying query or evaluation.</param>
        /// <returns>A task that completes with <see langword="true"/> when the execution must be suppressed, or
        /// <see langword="false"/> when the automation may proceed.</returns>
        Task<bool> IsBlockedAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}
