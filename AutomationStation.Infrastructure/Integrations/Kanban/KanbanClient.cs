using System.Net.Http.Json;

using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Integrations.Kanban.Requests;
using System.Net.Http.Json;

namespace AutomationStation.Infrastructure.Integrations.Kanban;

public sealed class KanbanClient(
    HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task CreatePlacementAsync(
        Guid entityId,
        Guid boardId,
        Guid columnId,
        KanbanRequestContext requestContext,
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
            requestContext.CorrelationId.ToString());

        message.Headers.Add(
            "Causation-Id",
            requestContext.CausationEventId.ToString());

        message.Headers.Add(
            "Actor-Id",
            requestContext.Actor.Id.ToString());

        message.Headers.Add(
            "Actor-Type",
            requestContext.Actor.Type.ToString());

        message.Headers.Add(
            "Idempotency-Key",
            requestContext.CommandId.ToString());

        using var response = await _httpClient.SendAsync(
            message,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}