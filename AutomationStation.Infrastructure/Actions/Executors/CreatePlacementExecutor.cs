using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Integrations.Kanban;

namespace AutomationStation.Infrastructure.Actions.Executors
{
    public sealed class CreatePlacementExecutor(
        KanbanClient kanbanClient,
        IEventValueResolver valueResolver)
        : IActionHandler
    {
        private readonly KanbanClient _kanbanClient = kanbanClient;
        private readonly IEventValueResolver _valueResolver = valueResolver;

        public string ActionType => "CreatePlacement";

        public TargetSystem TargetSystem => TargetSystem.Kanban;

        public async Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken)
        {
            if (!_valueResolver.TryGetValue(
                    context.Event,
                    "payload.entityId",
                    out var entityIdValue) ||
                !Guid.TryParse(
                    entityIdValue,
                    out var entityId))
            {
                throw new InvalidOperationException(
                    "Event does not contain a valid payload.entityId.");
            }

            var boardId =
                Guid.Parse(then.Parameters["boardId"]);

            var columnId =
                Guid.Parse(then.Parameters["columnId"]);

            var requestContext = new KanbanRequestContext(
                context.Event,
                context.ExecutionId);

            await _kanbanClient.CreatePlacementAsync(
                entityId,
                boardId,
                columnId,
                requestContext,
                cancellationToken);
        }
    }
}
