using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionPolicy
    {
        Task<AutomationPolicyResult> EvaluateAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}
