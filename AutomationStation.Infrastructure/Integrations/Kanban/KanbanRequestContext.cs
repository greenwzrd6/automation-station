using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Integrations.Kanban
{
    public sealed record KanbanRequestContext(
    Guid? CorrelationId,
    Guid? CausationEventId,
    Guid ExecutionId,
    Actor Actor);
}
