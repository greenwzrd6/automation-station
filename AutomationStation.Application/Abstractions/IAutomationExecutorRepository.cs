namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutorRepository
    {
        Task<bool> IsEnabledAsync(
            Guid executorId,
            CancellationToken cancellationToken);
    }
}