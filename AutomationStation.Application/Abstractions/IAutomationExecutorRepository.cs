using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutorRepository
    {
        Task<AutomationExecutor?> GetByIdAsync(
            Guid executorId,
            CancellationToken cancellationToken);
    }
}