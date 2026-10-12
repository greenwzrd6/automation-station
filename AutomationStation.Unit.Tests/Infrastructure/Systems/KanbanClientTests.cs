using System.Net;

using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Integrations.Kanban;

namespace AutomationStation.Unit.Tests.Infrastructure.Systems
{
    public class KanbanClientTests
    {
        [Fact]
        public async Task CreatePlacement_WithValidEndpoint_SendsPostRequest()
        {
            // Arrange
            var handler = CreateHandler();

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var kc = new KanbanClient(httpClient);

            var context = CreateContext();

            // Act
            await kc.CreatePlacementAsync(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                context,
                CancellationToken.None);

            // Assert
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal(
                "/api/placements/create",
                handler.RequestUri?.AbsolutePath);
        }

        [Fact]
        public async Task CreatePlacement_WithInvalidEndpoint_ThrowsException()
        {
            // Arrange
            var handler = CreateHandler();

            handler.Response = new HttpResponseMessage(
                HttpStatusCode.InternalServerError);

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var kc = new KanbanClient(httpClient);

            var context = CreateContext();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => kc.CreatePlacementAsync(
                    Guid.Empty,
                    Guid.Empty,
                    Guid.Empty,
                    context,
                    CancellationToken.None));
        }

        [Fact]
        public async Task CreatePlacement_WhenCalled_SendsHeaders()
        {
            // Arrange
            var handler = CreateHandler();
            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };
            var kc = new KanbanClient(httpClient);
            var context = CreateContext();

            // Act
            await kc.CreatePlacementAsync(
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                context,
                CancellationToken.None);

            // Assert
            Assert.Equal(
                context.CorrelationId.ToString(),
                handler.Request!.Headers.GetValues("Correlation-Id").Single());

            Assert.Equal(
                context.CausationEventId.ToString(),
                handler.Request!.Headers.GetValues("Causation-Id").Single());

            Assert.Equal(
                context.Actor.Id,
                handler.Request.Headers.GetValues("Actor-Id").Single());

            Assert.Equal(
                context.Actor.Type,
                handler.Request.Headers.GetValues("Actor-Type").Single());

            Assert.Equal(
                context.ExecutionId.ToString(),
                handler.Request.Headers.GetValues("Idempotency-Key").Single());
        }

        private static TestHttpMessageHandler CreateHandler()
        {
            return new TestHttpMessageHandler();
        }

        private static KanbanRequestContext CreateContext()
        {
            var actor = new Actor(
                "",
                "");

            var payload = System.Text.Json.JsonDocument
                .Parse("{}")
                .RootElement
                .Clone();

            var integrationEvent = new IntegrationEvent(
                Guid.Empty,
                "",
                SourceSystem.Kanban,
                0,
                Guid.Empty,
                null,
                actor,
                payload);

            return new KanbanRequestContext(
                integrationEvent,
                Guid.Empty);
        }
    }
}
