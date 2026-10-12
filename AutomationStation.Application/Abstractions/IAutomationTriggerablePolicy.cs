using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationTriggerablePolicy
    {
        AutomationPolicyResult Evaluate(
            EvaluationContext context);
    }
}
