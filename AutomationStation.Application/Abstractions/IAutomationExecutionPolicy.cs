using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionPolicy
    {
        Task<AutomationPolicyResult> EvaluateAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}
