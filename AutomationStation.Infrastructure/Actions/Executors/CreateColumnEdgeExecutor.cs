using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Infrastructure.Integrations.Kanban;

namespace AutomationStation.Infrastructure.Actions.Executors
{
    public sealed class CreateColumnEdgeExecutor(
        KanbanClient planningClient)
        : IActionHandler<ColumnActionContext>
    {
        public string ActionType => "CreateColumnEdge";

        public async Task ExecuteAsync(
            Then then,
            ColumnActionContext context,
            CancellationToken cancellationToken)
        {
            var toColumnId = Guid.Parse(
                then.Parameters["toColumnId"]);

            await planningClient.CreateColumnEdgeAsync(
                context.ColumnId,
                toColumnId,
                context.CausationEventId,
                cancellationToken);
        }
    }
}