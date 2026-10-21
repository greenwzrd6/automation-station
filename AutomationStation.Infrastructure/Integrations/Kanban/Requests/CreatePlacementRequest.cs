namespace AutomationStation.Infrastructure.Integrations.Kanban.Requests
{
    /// <summary>
    /// Record <c>CreatePlacementRequest</c> is the request body sent to the Kanban API to create placements.
    /// </summary>
    /// <remarks>Serialized as JSON and posted to /api/placements/create by <see
    /// cref="KanbanClient.CreatePlacementAsync"/>. When no adjacent entities are supplied, the placement is appended
    /// to the end of the target column.</remarks>
    /// <param name="EntityIds">Identifiers of the entities to place.</param>
    /// <param name="BoardId">Identifier of the board the placement belongs to.</param>
    /// <param name="ColumnId">Identifier of the column the entities are placed in.</param>
    /// <param name="AfterEntityIds">Identifiers of entities the new placement should follow; empty when unspecified.
    /// </param>
    /// <param name="BeforeEntityIds">Identifiers of entities the new placement should precede; empty when unspecified.
    /// </param>
    public sealed record CreatePlacementRequest(
        IReadOnlyCollection<Guid> EntityIds,
        Guid BoardId,
        Guid ColumnId,
        IReadOnlyCollection<Guid> AfterEntityIds,
        IReadOnlyCollection<Guid> BeforeEntityIds);
}