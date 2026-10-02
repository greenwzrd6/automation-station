namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionRepository
    {
        Task<Guid> GetOrCreateAsync(
            Guid automationId,
            Guid eventId,
            CancellationToken cancellationToken);
    }
}