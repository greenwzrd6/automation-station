using Automation.Core.Automations;

namespace Automation.Application.Abstractions;

public interface IAutomationRepository
{
    Task<IReadOnlyCollection<AutomationRule>>
        GetEnabledByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken);
}