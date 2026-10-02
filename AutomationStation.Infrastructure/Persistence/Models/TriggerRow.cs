using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Persistence.Models
{
    internal sealed record TriggerRow(
        Guid AutomationId,
        string EventType,
        SourceSystem SourceSystem);
}
