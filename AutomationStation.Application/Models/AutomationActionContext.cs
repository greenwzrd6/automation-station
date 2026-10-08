
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    public sealed record AutomationActionContext(
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Actor Actor => Event.Actor;
        public Guid CausationEventId => Event.EventId;
        public Guid CorrelationId => Event.CorrelationId;
    }
}
