#if false
using AutomationStation.Application.Abstractions;

namespace AutomationStation.Application.Models
{
    public sealed record ColumnActionContext(
        Guid ColumnId,
        Guid CausationEventId,
        Guid ExecutionId,
        Guid? CorrelationId)
        : IActionContext;
}
#endif
