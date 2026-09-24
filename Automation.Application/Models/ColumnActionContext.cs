using Automation.Application.Abstractions;

namespace Automation.Application.Models
{
    public sealed record ColumnActionContext(
        Guid ColumnId,
        string CausationEventId)
        : IActionContext;
}
