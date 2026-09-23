namespace Automation.Infrastructure.Integrations.Planning;

public sealed record CreatePlacementRequest(
    Guid EntityId,
    Guid BoardId,
    Guid ColumnId);