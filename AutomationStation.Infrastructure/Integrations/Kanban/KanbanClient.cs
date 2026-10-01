using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Integrations.Kanban.Requests;
using System.ComponentModel.Design;
using System.Net.Http.Json;

namespace AutomationStation.Infrastructure.Integrations.Kanban;

public sealed class KanbanClient(
    HttpClient httpClient)
{
    public async Task CreatePlacementAsync(
        Guid entityId,
        Guid boardId,
        Guid columnId,
        Guid correlationId,
        Guid causationEventId,
        Guid commandId,
        Actor actor,
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
            "Correlation-Id",
            correlationId.ToString());

        message.Headers.Add(
            "Causation-Id",
            causationEventId.ToString());

        message.Headers.Add(
            "Actor-Id",
            actor.Id.ToString());

        message.Headers.Add(
            "Actor-Type",
            actor.Type.ToString());

        message.Headers.Add(
            "Idempotency-Key",
            commandId.ToString());

        using var response = await httpClient.SendAsync(
            message,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    //public async Task CreateColumnEdgeAsync(
    //    Guid fromColumnId,
    //    Guid toColumnId,
    //    Guid correlationId,
    //    Guid causationEventId,
    //    Guid commandId,
    //    CancellationToken cancellationToken)
    //{
    //    var request = new CreateColumnEdgeRequest(
    //        fromColumnId,
    //        toColumnId);

    //    using var message = new HttpRequestMessage(
    //        HttpMethod.Post,
    //        "/api/columnedges/create")
    //    {
    //        Content = JsonContent.Create(request)
    //    };

    //    message.Headers.Add(
    //        "Correlation-Id",
    //        correlationId.ToString());

    //    message.Headers.Add(
    //        "Causation-Id",
    //        causationEventId.ToString());

    //    message.Headers.Add(
    //        "Idempotency-Key",
    //        commandId.ToString());

    //    using var response = await httpClient.SendAsync(
    //        message,
    //        cancellationToken);

    //    response.EnsureSuccessStatusCode();
    //}
}