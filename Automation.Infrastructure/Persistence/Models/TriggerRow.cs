namespace Automation.Infrastructure.Persistence.Models
{
    internal sealed record TriggerRow(
        Guid AutomationId,
        string Name,
        bool Enabled,
        string EventType,
        string? SourceSystem);
}
