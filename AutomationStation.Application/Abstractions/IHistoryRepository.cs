namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            Guid CausationEventId,
            CancellationToken CancellationToken);

        Task<bool> TriggeredRecentlyAsync(
            Guid AutomationId,
            DateTime Cooldown,
            CancellationToken CancellationToken);

        Task<bool> EventAlreadyProcessedAsync(
            Guid AutomationId,
            Guid CausationEventId,
            CancellationToken CancellationToken);
    }
}