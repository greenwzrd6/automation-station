using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Toj
{
    // This record represents the context of a TOJ request, including the integration event and execution ID.
    /// <summary>
    /// Pairs a TOJ integration event with the automation execution that produced it.
    /// </summary>
    /// <remarks>Projects the correlation, causation and actor metadata from the wrapped event so callers can pass the
    /// context around without reaching through to <see cref="Event"/> for each field.</remarks>
    /// <param name="Event">The integration event that triggered the request to TOJ.</param>
    /// <param name="ExecutionId">Identifier of the automation execution sending the request.</param>
    public sealed record TojRequestContext (
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;

        public Guid? CausationEventId => Event.CausationEventId;

        public Actor Actor => Event.Actor;
    }
}