namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            Guid CausationEventId,
            CancellationToken CancellationToken);

        Task<bool> HasTriggeredRecentlyAsync(
            Guid AutomationId,
            DateTime Cooldown,
            CancellationToken CancellationToken);

        Task<bool> HasCausationEventIdAsync(
            Guid AutomationId,
            Guid CausationEventId,
            CancellationToken CancellationToken);
    }
}