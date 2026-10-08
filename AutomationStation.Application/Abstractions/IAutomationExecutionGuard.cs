using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionGuard
    {
        Task<bool> IsBlockedAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}
