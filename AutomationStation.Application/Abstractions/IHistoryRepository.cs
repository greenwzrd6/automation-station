namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            Guid causationEventId,
            CancellationToken cancellationToken);

        Task<bool> HasTriggeredRecentlyAsync(
            Guid AutomationId,
            Guid causationEventId,
            DateTime cooldown,
            CancellationToken cancellationToken);
    }
}