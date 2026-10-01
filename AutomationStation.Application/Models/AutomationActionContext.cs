
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    public sealed record AutomationActionContext(
        IntegrationEvent Event,
        Guid? CausationEventId)
    {
        public Actor Actor => Event.Actor;
        public Guid? CorrelationId => Event.CorrelationId;
    }
}
