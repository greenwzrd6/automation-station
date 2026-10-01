namespace AutomationStation.Application.Models
{
    public sealed record EventBlockerContext(
        Guid EventId,
        string EventType,
        string Source,
        Guid? CorrelationId,
        Guid? CausationEventId);
}