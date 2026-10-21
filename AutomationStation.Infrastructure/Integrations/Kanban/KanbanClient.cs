using System.Net.Http.Json;

using AutomationStation.Infrastructure.Integrations.Kanban.Requests;

namespace AutomationStation.Infrastructure.Integrations.Kanban
{
    /// <summary>
    /// HTTP client used by automation actions to make changes in the kanban service.
    /// </summary>
    /// <remarks>The injected <see cref="HttpClient"/> is expected to be pre-configured with the kanban service's
    /// base address. Every request carries the correlation id, causation event id and actor taken from the
    /// <see cref="KanbanRequestContext"/>, plus an Idempotency-Key header holding the automation execution id so a
    /// retried execution of the same run can be recognised by the receiver. Responses are validated with
    /// EnsureSuccessStatusCode, so non-success status codes surface as exceptions rather than being ignored.</remarks>
    /// <param name="httpClient">Client pre-configured with the kanban service's base address.</param>
    public sealed class KanbanClient(
        HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Moves a single entity into a column by creating a placement through POST /api/placements/create.
        /// </summary>
        /// <remarks>The body contains only the one entity and no before/after anchors, so the kanban service
        /// resolves the position itself and appends the entity at the end of the target column's ordering. The call
        /// completes once a success status code has been received.</remarks>
        /// <param name="entityId">Identifier of the entity to place.</param>
        /// <param name="boardId">Identifier of the board that owns the target column.</param>
        /// <param name="columnId">Identifier of the column the entity is placed in.</param>
        /// <param name="requestContext">Supplies correlation id, causation event id, actor and the execution id
        /// written to the request headers.</param>
        /// <param name="cancellationToken">A token used to cancel the HTTP request.</param>
        /// <returns>A task that completes when the response has been received and validated.</returns>
        /// <exception cref="HttpRequestException">
        /// The request could not be sent, or the kanban service responded with a non-success status code.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The request is canceled through <paramref name="cancellationToken"/>.
        /// </exception>
        public async Task CreatePlacementAsync(
            Guid entityId,
            Guid boardId,
            Guid columnId,
            KanbanRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            var request = new CreatePlacementRequest(
                EntityIds: [entityId],
                BoardId: boardId,
                ColumnId: columnId,
                AfterEntityIds: [],
                BeforeEntityIds: []);

            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/placements/create")
            {
                Content = JsonContent.Create(request)
            };

            message.Headers.Add(
                "Correlation-Id",
                requestContext.CorrelationId.ToString());

            message.Headers.Add(
                "Causation-Id",
                requestContext.CausationEventId.ToString());

            message.Headers.Add(
                "Actor-Id",
                requestContext.Actor.Id);

            message.Headers.Add(
                "Actor-Type",
                requestContext.Actor.Type);

            message.Headers.Add(
                "Idempotency-Key",
                requestContext.ExecutionId.ToString());

            using var response = await _httpClient.SendAsync(
                message,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}