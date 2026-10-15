using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    internal sealed record TriggerRow(
        Guid AutomationId,
        string EventType,
        SourceSystem SourceSystem);
}
