namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            Guid CorrelationId,
            Guid? CausationEventId,
            CancellationToken CancellationToken);

        Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CorrelationId,
            CancellationToken CancellationToken);
    }
}