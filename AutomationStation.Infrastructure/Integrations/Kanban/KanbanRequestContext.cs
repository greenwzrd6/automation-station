using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Kanban
{
    public sealed record KanbanRequestContext(
        IntegrationEvent Event,
        Guid ExecutionId)
    {
        public Guid CorrelationId => Event.CorrelationId;
        public Guid? CausationEventId => Event.CausationEventId;
        public Actor Actor => Event.Actor;
    }
}