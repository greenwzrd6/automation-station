using Automation.Core.History;


namespace Automation.Application.Abstractions
{
    public interface IHistoryRepository
    {
        Task CreateAutomationTimestampAsync(
            Guid AutomationId,
            CancellationToken cancellationToken);

        Task<DateTime?> HasTriggeredSinceAsync(
            Guid AutomationId,
            DateTime since,
            CancellationToken cancellationToken);
    }
}