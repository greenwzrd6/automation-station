using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Infrastructure.Integrations.Kanban;

namespace AutomationStation.Infrastructure.Actions.Executors;

public sealed class CreatePlacementExecutor(
    KanbanClient planningClient)
    : IActionHandler<PlacementActionContext>
{
    public string ActionType => "CreatePlacement";

    public TargetSystem TargetSystem => throw new NotImplementedException();

    public bool CanHandle(string actionType)
    {
        throw new NotImplementedException();
    }

    public async Task ExecuteAsync(
        Then then,
        PlacementActionContext context,
        CancellationToken cancellationToken)
    {
        var boardId = Guid.Parse(
            then.Parameters["boardId"]);

        var columnId = Guid.Parse(
            then.Parameters["columnId"]);

        var commandId = Guid.NewGuid();

        await planningClient.CreatePlacementAsync(
            context.EntityId,
            boardId,
            columnId,
            context.CorrelationId,
            context.CausationEventId,
            commandId,
            context.Actor,
            cancellationToken);
    }
}