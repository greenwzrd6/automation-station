
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    public sealed record AutomationActionContext(
        IntegrationEvent Event,
        Guid CorrelationId,
        Guid ExecutionId)
    {
        public Actor Actor => Event.Actor;

        public Guid CausationEventId => Event.EventId;
    }
}
