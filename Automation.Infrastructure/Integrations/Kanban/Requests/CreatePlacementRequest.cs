namespace Automation.Infrastructure.Integrations.Kanban.Requests;

public sealed record CreatePlacementRequest(
    IReadOnlyCollection<Guid> EntityIds,
    Guid BoardId,
    Guid ColumnId,
    IReadOnlyCollection<Guid> AfterEntityIds,
    IReadOnlyCollection<Guid> BeforeEntityIds);