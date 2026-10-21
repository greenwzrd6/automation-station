using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Infrastructure.Integrations.Toj;

namespace AutomationStation.Infrastructure.Actions.Executors
{
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

        /// <summary>
        /// Creates a task in TOJ under the same root as the entity referenced by the triggering event.
        /// </summary>
        /// <param name="then">The action definition; its <c>Parameters</c> must contain a non-empty "title" and its
        /// <c>ExecutorId</c> must be set to the executor performing the creation.</param>
        /// <param name="context">The triggering event; its JSON payload must contain an <c>entityId</c> property with
        /// the GUID of the source task whose root the new task is created under.</param>
        /// <param name="cancellationToken">A token used to cancel the TOJ API call.</param>
        /// <returns>A task that completes when TOJ has accepted the task creation.</returns>
        /// <exception cref="ArgumentException">The event payload is missing a valid <c>entityId</c>, or the action
        /// has no "title" parameter.</exception>
        /// <exception cref="InvalidOperationException">The action definition has no executor identifier.</exception>
        /// <exception cref="HttpRequestException">TOJ returned a non-success status code for the create request.</exception>
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
}