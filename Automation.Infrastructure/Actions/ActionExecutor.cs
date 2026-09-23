using System.Net.Http.Json;
using Automation.Application.Abstractions;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Actions;

public sealed class ActionExecutor(
    HttpClient httpClient) : IActionExecutor
{
    public async Task ExecuteAsync(
        Then then,
        ActionContext context,
        CancellationToken cancellationToken)
    {
        if (then.Type != "CreatePlacement")
        {
            throw new NotSupportedException(
                $"Unsupported action: {then.Type}");
        }

        var boardId = Guid.Parse(
            then.Parameters["boardId"]);

        var columnId = Guid.Parse(
            then.Parameters["columnId"]);

        var sourceColumnId = then.Parameters.TryGetValue(
            "sourceColumnId",
            out var sourceColumnIdString)
            ? Guid.Parse(sourceColumnIdString)
            : (Guid?)null;

        var request = new
        {
            EntityIds = new[] { context.EntityId },
            BoardId = boardId,
            ColumnId = columnId,
            AfterEntityIds = Array.Empty<Guid>(),
            BeforeEntityIds = Array.Empty<Guid>(),
            SourceColumnId = sourceColumnId
        };

        await Task.Delay(3000, cancellationToken);

        using var response = await httpClient.PostAsJsonAsync(
            "/api/placements/create",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}