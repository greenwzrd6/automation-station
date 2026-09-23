using System.Net.Http.Json;

namespace Automation.Infrastructure.Integrations.Planning;

public sealed class PlanningClient(
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
            entityId,
            boardId,
            columnId);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/placements/create")
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