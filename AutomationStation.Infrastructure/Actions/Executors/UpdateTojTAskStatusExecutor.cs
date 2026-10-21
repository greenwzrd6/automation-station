using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Infrastructure.Integrations.Toj;

namespace AutomationStation.Infrastructure.Actions.Executors
{
    /// <summary>
    /// Handles UpdateTaskStatus actions by validating inputs and invoking TojClient to update a TOJ task's status.
    /// </summary>
    /// <remarks>Validates that the triggering event contains a valid entityId, that then.Parameters contains
    /// a valid statusId, and that then.ExecutorId has a value before calling TojClient.UpdateTaskStatusAsync.</remarks>
    /// <param name="tojClient">Client used to send task status updates to the TOJ system.</param>
    public sealed class UpdateTojTaskStatusExecutor(
        TojClient tojClient)
    : IActionHandler
    {
        public string ActionType => "UpdateTaskStatus";

        public TargetSystem TargetSystem => TargetSystem.TojSystem;

        /// <summary>
        /// Determines whether this executor is responsible for the given action type.
        /// </summary>
        /// <param name="actionType">The action type to test; the comparison ignores casing.</param>
        /// <returns><see langword="true"/> when <paramref name="actionType"/> matches <see cref="ActionType"/>;
        /// otherwise <see langword="false"/>.</returns>
        public bool CanHandle(string actionType)
        {
            return string.Equals(actionType, ActionType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Updates the status of the TOJ task identified by the triggering event's entityId.
        /// </summary>
        /// <remarks>The task ID is read from the "entityId" property of the event payload, and the new status from
        /// the "statusId" entry of the action's parameters. Both are validated before the TOJ client is invoked, and
        /// the update is attributed to the action's executor.</remarks>
        /// <param name="then">The action definition carrying the statusId parameter and the executor ID to attribute the update to.</param>
        /// <param name="context">The action context; its event payload must contain the entityId of the task to update.</param>
        /// <param name="cancellationToken">A token used to cancel the call to the TOJ system.</param>
        /// <returns>A task that completes when the status update has been sent.</returns>
        /// <exception cref="ArgumentException">
        /// The event payload has no parseable "entityId", or <paramref name="then"/> has no parseable "statusId" parameter.
        /// </exception>
        /// <exception cref="InvalidOperationException"><paramref name="then"/> has no ExecutorId.</exception>
        public async Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken)
        {
            if (!context.Event.Payload.TryGetProperty(
                    "entityId",
                    out var entityIdElement) ||
                !Guid.TryParse(
                    entityIdElement.GetString(),
                    out var taskId))
            {
                throw new ArgumentException(
                    "The triggering event must contain " +
                    "a valid entityId.");
            }

            if (!then.Parameters.TryGetValue(
                    "statusId",
                    out var statusIdValue) ||
                !int.TryParse(
                    statusIdValue,
                    out var statusId))
            {
                throw new ArgumentException(
                    "The UpdateTaskStatus action requires " +
                    "a valid statusId.");
            }

            if (!then.ExecutorId.HasValue)
            {
                throw new InvalidOperationException("The action requires an executor.");
            }

            Guid executorId = then.ExecutorId.Value;

            await tojClient.UpdateTaskStatusAsync(
                taskId,
                statusId,
                executorId,
                cancellationToken);
        }
    }
}
