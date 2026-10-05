using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Models
{
    public sealed record EventBlockerContext(
        Guid EventId,
        string EventType,
        SourceSystem Source,
        Guid CorrelationId,
        Guid? CausationEventId,
        Actor Actor);
}