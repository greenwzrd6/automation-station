using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Policies
{
    public sealed class AutomationTriggerablePolicy
        : IAutomationTriggerablePolicy
    {
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