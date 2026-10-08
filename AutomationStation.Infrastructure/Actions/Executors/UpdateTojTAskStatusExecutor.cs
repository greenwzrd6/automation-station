using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Integrations.Toj;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Infrastructure.Actions.Executors
{
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

            await tojClient.UpdateTaskStatusAsync(
                taskId,
                statusId,
                cancellationToken);
        }
    }
}
