using Automation.Infrastructure.Integrations.Planning.Requests;
using System.Net.Http.Json;

namespace Automation.Infrastructure.Integrations.Kanban;

public sealed class KanbanClient(
    HttpClient httpClient)
{
    public async Task CreatePlacementAsync(
        Guid entityId,
        Guid boardId,
        Guid columnId,
        string causationEventId,
        CancellationToken cancellationToken)
    {
        var request = new CreatePlacementRequest(
            EntityIds: [entityId],
            BoardId: boardId,
            ColumnId: columnId,
            AfterEntityIds: [],
            BeforeEntityIds: []);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/placements/create")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(
            "Idempotency-Key",
            causationEventId);

        using var response = await httpClient.SendAsync(
            message,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task CreateColumnEdgeAsync(
    Guid fromColumnId,
    Guid toColumnId,
    string causationEventId,
    CancellationToken cancellationToken)
    {
        var request = new CreateColumnEdgeRequest(
            fromColumnId,
            toColumnId);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/columnedges/create")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(
            "Idempotency-Key",
            causationEventId);

        using var response = await httpClient.SendAsync(
            message,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}