using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Models
{
    public sealed record EventBlockerContext(
        IntegrationEvent Event)
    {
        public Guid Id;
        public Guid EventId => Event.EventId;
        public string EventType => Event.EventType;
        public SourceSystem Source => Event.Source;
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}