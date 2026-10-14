using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Toj
{
    // This record represents the context of a TOJ request, including the integration event and execution ID.
    public sealed record TojRequestContext (
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}