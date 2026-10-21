using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Infrastructure.Integrations.Kanban;

namespace AutomationStation.Infrastructure.Actions.Executors
{
    /// <summary>
    /// Class <c>CreatePlacementExecutor</c> is responsible for executing the "CreatePlacement" action.
    /// </summary>
    /// <param name="kanbanClient">The system client for interacting with the Kanban system.</param>
    /// <param name="valueResolver">The resolver for extracting values from the event.</param>
    public sealed class CreatePlacementExecutor(
        KanbanClient kanbanClient,
        IEventValueResolver valueResolver)
        : IActionHandler
    {
        private readonly KanbanClient _kanbanClient = kanbanClient;
        private readonly IEventValueResolver _valueResolver = valueResolver;

        public string ActionType => "CreatePlacement";

        public TargetSystem TargetSystem => TargetSystem.Kanban;

        /// <summary>
        /// Creates a placement in the Kanban system for the entity referenced by the triggering event.
        /// </summary>
        /// <remarks>The entity id is read from the event payload (<c>payload.entityId</c>), while the board
        /// and column to place it in come from the action's parameters. The event's correlation, causation, and
        /// actor metadata are forwarded to the Kanban request so the created placement can be traced back to
        /// this automation run.</remarks>
        /// <param name="then">The configured action; its parameters must contain the <c>boardId</c> and
        /// <c>columnId</c> GUIDs identifying where the entity is placed.</param>
        /// <param name="context">The action context providing the triggering event and the execution id.</param>
        /// <param name="cancellationToken">A token used to cancel the Kanban request while it is in flight.</param>
        /// <returns>A task that completes when the placement has been created in the Kanban system.</returns>
        /// <exception cref="InvalidOperationException">The event payload lacks a <c>payload.entityId</c> value,
        /// or the value is not a valid GUID.</exception>
        /// <exception cref="FormatException">A <c>boardId</c> or <c>columnId</c> parameter is not a valid
        /// GUID.</exception>
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
