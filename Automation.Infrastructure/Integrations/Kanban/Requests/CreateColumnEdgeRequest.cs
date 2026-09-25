namespace Automation.Infrastructure.Integrations.Kanban.Requests
{
    public sealed record CreateColumnEdgeRequest(
    Guid FromColumnId,
    Guid ToColumnId);
}
