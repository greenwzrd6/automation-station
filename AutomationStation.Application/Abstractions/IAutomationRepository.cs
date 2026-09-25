using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions;

public interface IAutomationRepository
{
    Task<IReadOnlyCollection<Core.Automations.Automation>>
        GetEnabledByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken);
}