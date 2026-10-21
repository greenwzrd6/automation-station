using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Kanban
{
    /// <summary>
    /// Carries the originating integration event and execution identifier for work performed on behalf of the kanban
    /// system.
    /// </summary>
    /// <remarks>Exposes the correlation id, causation event id, and actor of the wrapped event so that work performed
    /// during the execution can be correlated with the event that caused it without unwrapping the event everywhere.
    /// </remarks>
    /// <param name="Event">The integration event that triggered this request.</param>
    /// <param name="ExecutionId">Identifier of the current automation execution.</param>
    public sealed record KanbanRequestContext(
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;
        public Guid CausationEventId => Event.EventId;
        public Actor Actor => Event.Actor;
    }
}