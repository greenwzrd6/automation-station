using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Policies
{
    public sealed class AutomationTriggerablePolicy
        : IAutomationExecutionPolicy
    {
        public Task<AutomationPolicyResult> EvaluateAsync(
            LoopBlockerContext context,
            CancellationToken cancellationToken)
        {
            var triggeredByAutomation =
                string.Equals(
                    context.Event.Actor.Type,
                    // Ändra tillbaka när vi faktiskt får toj data ( tock och jolltortyr )
                    //"AutomationExecutor",
                    "",
                    StringComparison.OrdinalIgnoreCase);

            if (triggeredByAutomation &&
                !context.Automation.AutomationTriggerable)
            {
                return Task.FromResult(
                    AutomationPolicyResult.Block(
                        "Automation cannot be triggered by another automation."));
            }

            return Task.FromResult(
                AutomationPolicyResult.Allow());
        }
    }
}