using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Services;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Whens;
using System.Text.Json;

namespace AutomationStation.Unit.Tests.Application.Services
{
    public sealed class ConditionEvaluatorTests
    {
        private readonly EventValueResolver _valueResolver = new();
        private readonly ConditionEvaluator _evaluator;

        public ConditionEvaluatorTests()
        {
            _evaluator = new ConditionEvaluator(_valueResolver);
        }

        [Fact]
        public void Matches_WhenEqualsOperatorAndValuesMatch_ReturnsTrue()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated");
            var condition = new Condition("eventtype", ConditionOperator.Equals, "PlacementCreated");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_WhenEqualsOperatorIsCaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated");
            var condition = new Condition("eventtype", ConditionOperator.Equals, "placementcreated");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_WhenEqualsOperatorAndValuesDiffer_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent("PacmanEated");
            var condition = new Condition("eventtype", ConditionOperator.Equals, "PlacementCreated");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Matches_WhenNotEqualsOperatorAndValuesDiffer_ReturnsTrue()
        {
            // Arrange
            var integrationEvent = CreateEvent("PacmanEated");
            var condition = new Condition("eventtype", ConditionOperator.NotEquals, "PlacementCreated");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_WhenNotEqualsOperatorAndValuesMatch_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated");
            var condition = new Condition("eventtype", ConditionOperator.NotEquals, "PlacementCreated");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Matches_WhenFieldDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated");
            var condition = new Condition("birgerjarl", ConditionOperator.NotEquals, "pacman");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Matches_WhenFieldIsPayloadPath_ResolvesFromPayload()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated", payloadJson: """{ "status": "Active" }""");
            var condition = new Condition("payload.status", ConditionOperator.Equals, "Active");

            // Act
            var result = _evaluator.Matches(condition, integrationEvent);

            // Assert
            Assert.True(result);
        }

        private static IntegrationEvent CreateEvent(
            string eventType,
            string payloadJson = "{}")
        {
            var payload = JsonDocument
                .Parse(payloadJson)
                .RootElement
                .Clone();

            return new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: eventType,
                Source: SourceSystem.Kanban,
                CompanyId: 0,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("Test Actor", "Test Actor Type"),
                Payload: payload);
        }
    }
}
