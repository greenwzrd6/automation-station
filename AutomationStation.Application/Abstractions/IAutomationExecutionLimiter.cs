using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionLimiter
    {
        Task<bool> TryRecordExecutionAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}