using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Integrations.Toj;

namespace AutomationStation.Infrastructure.Actions.Executors;

/// <summary>
/// Handles CreateTask actions by creating a corresponding task in the Toj system using the provided TojClient.
/// </summary>
/// <remarks>Requires the triggering event to contain a valid entityId, a non-empty title parameter, and a
/// specified executor. Throws ArgumentException if entityId is missing or the title is empty; throws
/// InvalidOperationException if ExecutorId is not set.</remarks>
/// <param name="tojClient">Client for interacting with the Toj system to create tasks.</param>
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

        if (!then.ExecutorId.HasValue)
        {
            throw new InvalidOperationException("The action requires an executor.");
        }

        Guid executorId = then.ExecutorId.Value;

        await tojClient.CreateTaskInSameRootAsync(
            sourceTaskId,
            title,
            executorId,
            cancellationToken);
    }
}