using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Toj
{
    public sealed record TojRequestContext (
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}