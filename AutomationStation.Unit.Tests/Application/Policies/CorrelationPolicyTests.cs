using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Policies;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using Moq;

namespace AutomationStation.Unit.Tests.Application.Policies
{
    public class CorrelationPolicyTests
    {
        private readonly Mock<IHistoryRepository> _historyRepositoryMock = new();
        private readonly CorrelationPolicy _cp;
        public CorrelationPolicyTests()
        {
            _cp = new CorrelationPolicy(_historyRepositoryMock.Object);
        }

        [Fact]
        public async Task Evaluate_WhenEventNotProcessed_IsAllowed()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            
            var context = CreateContext(automationId, correlationId);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _cp.EvaluateAsync(
                context,
                CancellationToken.None);

            // Assert
            Assert.True(result.Allowed);

            _historyRepositoryMock.
                Verify(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Evaluate_WhenEventAlreadyProcessed_IsBlocked()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _cp.EvaluateAsync(
                context,
                CancellationToken.None);

            // Assert
            Assert.False(result.Allowed);
            Assert.Equal("Correlation event already processed.", result.Reason);

            _historyRepositoryMock.
                Verify(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static EvaluationContext CreateContext(
            Guid automationId,
            Guid correlationId)
        {
            var when = new When(
                "TestEvent",
                SourceSystem.Kanban,
                []);

            var thens = new[]
            {
            new Then(
                "TestAction",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
            };

            var automation = new Automation(
                automationId,
                "TestAutomation",
                true,
                true,
                when,
                thens);

            var actor = new Actor(
                "", 
                "");

            var payload = System.Text.Json.JsonDocument
                .Parse("{}")
                .RootElement
                .Clone();

            var integrationEvent = new IntegrationEvent(
                Guid.NewGuid(),
                "TestEvent",
                SourceSystem.Kanban,
                0,
                correlationId,
                null,
                actor,
                payload);

            return new EvaluationContext(automation, integrationEvent);
        }
    }
}
