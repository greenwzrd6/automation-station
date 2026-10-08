
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    public sealed record AutomationActionContext(
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}
