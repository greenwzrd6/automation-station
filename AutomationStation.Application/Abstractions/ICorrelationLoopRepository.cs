using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions
{
    public interface ICorrelationLoopRepository
    {
        Task<bool> IsBlockedAsync(
            Automation automation,
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken);

        //Task CleanupOldEventsAsync(
        //    TimeSpan maxAge,
        //    CancellationToken cancellationToken);
    }
}