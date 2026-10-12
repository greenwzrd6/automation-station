using Moq;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Policies;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

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
        public async Task EvaluateAsync_WhenEventNotProcessed_IsAllowed()
        {
            // Arrange
            var automationId = Guid.Empty;
            var correlationId = Guid.Empty;
            
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
        public async Task EvaluateAsync_WhenEventAlreadyProcessed_IsBlocked()
        {
            // Arrange
            var automationId = Guid.Empty;
            var correlationId = Guid.Empty;

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
            Assert.Equal("Automation already executed in this correlation.", result.Reason);

            _historyRepositoryMock.
                Verify(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EvaluateAsync_WhenCorrelationIdIsDifferent_IsAllowed()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId1 = Guid.NewGuid();
            var correlationId2 = Guid.NewGuid();

            var context1 = CreateContext(
                automationId,
                correlationId1);

            var context2 = CreateContext(
                automationId,
                correlationId2);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result1 = await _cp.EvaluateAsync(
                context1,
                CancellationToken.None);

            var result2 = await _cp.EvaluateAsync(
                context2,
                CancellationToken.None);

            // Assert
            Assert.True(result1.Allowed);
            Assert.True(result2.Allowed);

            _historyRepositoryMock.Verify(
                x => x.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId1,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _historyRepositoryMock.Verify(
                x => x.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId2,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EvaluateAsync_WhenAutomationIdIsDifferent_IsAllowed()
        {
            // Arrange
            var automationId1 = Guid.NewGuid();
            var automationId2 = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context1 = CreateContext(
                automationId1,
                correlationId);

            var context2 = CreateContext(
                automationId2,
                correlationId);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    automationId1,
                    correlationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    automationId2,
                    correlationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result1 = await _cp.EvaluateAsync(
                context1,
                CancellationToken.None);

            var result2 = await _cp.EvaluateAsync(
                context2,
                CancellationToken.None);

            // Assert
            Assert.True(result1.Allowed);
            Assert.True(result2.Allowed);

            _historyRepositoryMock.Verify(
                x => x.EventAlreadyProcessedAsync(
                    automationId1,
                    correlationId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _historyRepositoryMock.Verify(
                x => x.EventAlreadyProcessedAsync(
                    automationId2,
                    correlationId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EvaluateAsync_WhenUsed_ForwardsCancellationToken()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            _historyRepositoryMock
                .Setup(x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    cancellationToken))
                .ReturnsAsync(false);
            // Act
            var result = await _cp.EvaluateAsync(
                context,
                cancellationToken);
            // Assert
            Assert.True(result.Allowed);
            _historyRepositoryMock.Verify(
                x => x.EventAlreadyProcessedAsync(
                    context.AutomationId,
                    context.CorrelationId,
                    cancellationToken),
                Times.Once);
        }

        private static EvaluationContext CreateContext(
            Guid automationId,
            Guid correlationId)
        {
            var when = new When(
                "",
                SourceSystem.Kanban,
                []);

            var thens = new[]
            {
            new Then(
                "",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
            };

            var automation = new Automation(
                automationId,
                "",
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
                Guid.Empty,
                "",
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
