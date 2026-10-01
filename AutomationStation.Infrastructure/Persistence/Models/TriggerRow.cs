namespace AutomationStation.Infrastructure.Persistence.Models
{
    internal sealed record TriggerRow(
        Guid AutomationId,
        string EventType,
        string? SourceSystem);
}
