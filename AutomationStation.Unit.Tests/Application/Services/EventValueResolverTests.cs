using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Services;
using AutomationStation.Core.Automations.Systems;
using System.Text.Json;

namespace AutomationStation.Unit.Tests.Application.Services
{
    public sealed class EventValueResolverTests
    {
        private readonly EventValueResolver _resolver = new();

        [Fact]
        public void TryGetValue_WhenFieldIsNull_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var result = _resolver.TryGetValue(integrationEvent, null, out var value);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsWhitespace_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var result = _resolver.TryGetValue(integrationEvent, " ", out var value);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsEventType_ReturnsEventType()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "eventtype", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("PlacementCreated", value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsSource_ReturnsSourceName()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "source", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("Kanban", value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsCompanyId_ReturnsCompanyIdAsString()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "companyid", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("0", value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsEventId_ReturnsEventIdAsString()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId,
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "eventid", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal(eventId.ToString(), value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsCorrelationId_ReturnsCorrelationIdAsString()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId);

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "correlationid", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal(correlationId.ToString(), value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsCausationEventId_WhenNotNull_ReturnsIdAsString()
        {
            // Arrange
            var causationEventId = Guid.NewGuid();
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                causationEventId: causationEventId);

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "causationEventId", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal(causationEventId.ToString(), value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsCausationEventId_WhenNull_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "causationEventId", out var value);

            // Assert
            Assert.False(found);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsActorId_ReturnsActorId()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "actor.id", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("-1", value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsActorType_ReturnsActorType()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "actor.type", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("TestActorType", value);
        }

        [Fact]
        public void TryGetValue_WhenFieldIsUnknown_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "korven", out var value);

            // Assert
            Assert.False(found);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathExists_ReturnsStringValue()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                payloadJson: """{ "status": "Active" }""");

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.status", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("Active", value);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathIsNested_ReturnsStringValue()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                payloadJson: """{ "placement": { "status": "Active" } }""");

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.placement.status", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("Active", value);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathIsNumber_ReturnsRawNumber()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                payloadJson: """{ "status": 1 }""");

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.status", out var value);

            // Assert
            Assert.True(found);
            Assert.Equal("1", value);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathIsNull_ReturnsTrueWithNullValue()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                payloadJson: """{ "status": null }""");

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.status", out var value);

            // Assert
            Assert.True(found);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid());

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.status", out var value);

            // Assert
            Assert.False(found);
        }

        [Fact]
        public void TryGetValue_WhenPayloadPathTraversesNonObject_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent(
                "PlacementCreated",
                eventId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                payloadJson: """{ "count": 5 }""");

            // Act
            var found = _resolver.TryGetValue(integrationEvent, "payload.count.name", out var value);

            // Assert
            Assert.False(found);
            Assert.Null(value);
        }

        private static IntegrationEvent CreateEvent(
            string eventType,
            Guid eventId,
            Guid correlationId,
            string payloadJson = "{}",
            Guid? causationEventId = null)
        {
            var payload = JsonDocument
                .Parse(payloadJson)
                .RootElement
                .Clone();

            return new IntegrationEvent(
                EventId: eventId,
                EventType: eventType,
                Source: SourceSystem.Kanban,
                CompanyId: 0,
                CorrelationId: correlationId,
                CausationEventId: causationEventId,
                Actor: new Actor("-1", "TestActorType"),
                Payload: payload);
        }
    }
}
