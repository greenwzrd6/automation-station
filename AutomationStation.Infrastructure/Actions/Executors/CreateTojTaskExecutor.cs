using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Integrations.Toj;

namespace AutomationStation.Infrastructure.Actions.Executors;

public sealed class CreateTojTaskExecutor(
    TojClient tojClient)
    : IActionHandler
{
    public string ActionType => "CreateTask";

    public TargetSystem TargetSystem =>
        TargetSystem.TojSystem;

    public bool CanHandle(string actionType)
    {
        return string.Equals(
            ActionType,
            actionType,
            StringComparison.OrdinalIgnoreCase);
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
                out var sourceTaskId))
        {
            throw new ArgumentException(
                "The triggering event must contain " +
                "a valid entityId.");
        }

        if (!then.Parameters.TryGetValue(
                "title",
                out var title) ||
            string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "The CreateTask action requires a title.");
        }

        await tojClient.CreateTaskInSameRootAsync(
            sourceTaskId,
            title,
            cancellationToken);
    }
}