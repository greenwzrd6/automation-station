using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Policies
{
    /// <summary>
    /// Policy that decides whether an automation may run when the triggering event was produced by another
    /// automation.
    /// </summary>
    /// <remarks>Most automations cannot be triggered by other automations, which prevents unintended
    /// automation chains. An automation opts into chaining by setting <c>AutomationTriggerable</c> on its
    /// configuration. The actor type is matched case-insensitively against "Automation".</remarks>
    public sealed class AutomationTriggerablePolicy
        : IAutomationTriggerablePolicy
    {
        /// <summary>
        /// Evaluates whether the automation is allowed to run for the given event.
        /// </summary>
        /// <remarks>Returns a blocking result only when the event's actor type identifies an automation and
        /// the automation has not been marked triggerable; in all other cases the automation is allowed to
        /// run.</remarks>
        /// <param name="context">The evaluation context carrying the automation under evaluation and the
        /// triggering event.</param>
        /// <returns>An allow result when the automation may run, or a block result with an explanatory reason
        /// when chaining is not permitted.</returns>
        public AutomationPolicyResult Evaluate(
            EvaluationContext context)
        {
            var triggeredByAutomation =
                string.Equals(
                    context.Event.Actor.Type,
                    // Ändra tillbaka när vi faktiskt får toj data ( tock och jolltortyr )
                    //"AutomationExecutor",
                    "Automation",
                    StringComparison.OrdinalIgnoreCase);

            if (triggeredByAutomation &&
                !context.Automation.AutomationTriggerable)
            {
                return AutomationPolicyResult.Block(
                    "Automation cannot be triggered by another automation.");
            }

            return AutomationPolicyResult.Allow();
        }
    }
}