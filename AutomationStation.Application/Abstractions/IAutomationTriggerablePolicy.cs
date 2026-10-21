using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Decides whether an automation is allowed to be triggered by a given integration event.
    /// </summary>
    /// <remarks>Used before an automation's conditions are evaluated to guard against unwanted triggering, for example
    /// preventing an automation from being re-triggered by an event that another automation produced.</remarks>
    public interface IAutomationTriggerablePolicy
    {
        /// <summary>
        /// Evaluates whether the automation in <paramref name="context"/> may be triggered by the accompanying event.
        /// </summary>
        /// <param name="context">The automation to evaluate together with the integration event that would trigger it.</param>
        /// <returns>An <see cref="AutomationPolicyResult"/> whose <c>Allowed</c> flag indicates the decision; when the
        /// automation may not trigger, the result's reason states why.</returns>
        AutomationPolicyResult Evaluate(
            EvaluationContext context);
    }
}
