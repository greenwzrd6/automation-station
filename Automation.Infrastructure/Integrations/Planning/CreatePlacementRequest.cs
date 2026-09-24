namespace Automation.Infrastructure.Integrations.Planning;

public sealed record CreatePlacementRequest(
    IReadOnlyCollection<Guid> EntityIds,
    Guid BoardId,
    Guid ColumnId,
    IReadOnlyCollection<Guid> AfterEntityIds,
    IReadOnlyCollection<Guid> BeforeEntityIds);