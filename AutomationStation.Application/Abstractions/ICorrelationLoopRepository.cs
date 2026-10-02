namespace AutomationStation.Application.Abstractions
{
    public interface ICorrelationLoopRepository
    {
        Task<bool> IsBlockedAsync(
            Guid correlationId,
            Guid eventId,
            CancellationToken cancellationToken);

        Task CleanupOldEventsAsync(
            TimeSpan maxAge,
            CancellationToken cancellationToken);
    }
}