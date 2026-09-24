namespace Automation.Infrastructure.Integrations.Planning.Requests
{
    public sealed record CreateColumnEdgeRequest(
    Guid FromColumnId,
    Guid ToColumnId);
}
