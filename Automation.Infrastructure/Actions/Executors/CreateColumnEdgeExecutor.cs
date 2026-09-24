using Automation.Application.Models;
using Automation.Core.Automations;
using Automation.Infrastructure.Integrations.Planning;

namespace Automation.Infrastructure.Actions.Executors
{
    public sealed class CreateColumnEdgeExecutor(
        PlanningClient planningClient)
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