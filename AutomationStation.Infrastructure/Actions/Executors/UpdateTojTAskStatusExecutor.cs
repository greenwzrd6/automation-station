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

        public bool CanHandle(string actionType)
        {
            return string.Equals(actionType, ActionType, StringComparison.OrdinalIgnoreCase);
        }

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
