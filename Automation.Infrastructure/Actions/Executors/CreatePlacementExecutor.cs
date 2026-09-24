using Automation.Application.Models;
using Automation.Core.Automations;
using Automation.Infrastructure.Integrations.Kanban;

namespace Automation.Infrastructure.Actions.Executors;

public sealed class CreatePlacementExecutor(
    KanbanClient planningClient)
    : IActionHandler<PlacementActionContext>
{
    public string ActionType => "CreatePlacement";

    public async Task ExecuteAsync(
        Then then,
        PlacementActionContext context,
        CancellationToken cancellationToken)
    {
        var boardId = Guid.Parse(
            then.Parameters["boardId"]);

        var columnId = Guid.Parse(
            then.Parameters["columnId"]);

        await planningClient.CreatePlacementAsync(
            context.EntityId,
            boardId,
            columnId,
            context.CausationEventId,
            cancellationToken);
    }
}