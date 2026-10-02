using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionGuard
    {
        Task<bool> IsBlockedAsync(
            Automation automation,
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken);
    }
}
