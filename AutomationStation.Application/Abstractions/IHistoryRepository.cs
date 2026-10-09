using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);

        Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CorrelationId,
            CancellationToken cancellationToken);
    }
}