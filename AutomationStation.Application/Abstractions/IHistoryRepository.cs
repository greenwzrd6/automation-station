namespace AutomationStation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            CancellationToken cancellationToken);

        Task<bool> HasTriggeredRecentlyAsync(
            Guid AutomationId,
            DateTime cooldown,
            CancellationToken cancellationToken);
    }
}