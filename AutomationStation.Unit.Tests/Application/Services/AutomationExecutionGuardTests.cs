using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Services;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Core.Automations.Whens;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutomationStation.Unit.Tests.Application.Services
{
    public sealed class AutomationExecutionGuardTests
    {
        private readonly Mock<IAutomationExecutionPolicy> _policy1Mock = new();
        private readonly Mock<IAutomationExecutionPolicy> _policy2Mock = new();
        private readonly Mock<ILogger<AutomationExecutionGuard>> _loggerMock = new();
        private readonly AutomationExecutionGuard _guard;

        public AutomationExecutionGuardTests()
        {
            _guard = new AutomationExecutionGuard(
                [_policy1Mock.Object, _policy2Mock.Object],
                _loggerMock.Object);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenAllPoliciesAllow_ReturnsFalse()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            _policy1Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            _policy2Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            // Act

            var blocked = await _guard.IsBlockedAsync(
                context,
                CancellationToken.None);

            // Assert
            Assert.False(blocked);

            _policy1Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _policy2Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenAnyPolicyBlocks_ReturnsTrue()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            _policy1Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            _policy2Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Block("loop detected"));

            // Act

            var blocked = await _guard.IsBlockedAsync(
                context,
                CancellationToken.None);

            // Assert
            Assert.True(blocked);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenPolicyBlocks_LaterPoliciesAreNotEvaluated()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            _policy1Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Block("loop detected"));

            _policy2Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            // Act

            var blocked = await _guard.IsBlockedAsync(
                context,
                CancellationToken.None);

            // Assert
            Assert.True(blocked);

            _policy1Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _policy2Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenUsed_ForwardsCancellationToken()
        {
            // Arrange
            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            _policy1Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    cancellationToken))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            _policy2Mock
                .Setup(x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    cancellationToken))
                .ReturnsAsync(AutomationPolicyResult.Allow());

            // Act
            var blocked = await _guard.IsBlockedAsync(
                context,
                cancellationToken);

            // Assert
            Assert.False(blocked);

            _policy1Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    cancellationToken),
                Times.Once);

            _policy2Mock.Verify(
                x => x.EvaluateAsync(
                    It.IsAny<EvaluationContext>(),
                    cancellationToken),
                Times.Once);
        }

        private static EvaluationContext CreateContext(
            Guid automationId,
            Guid correlationId)
        {
            var when = new When(new Trigger("Test When",
                SourceSystem.Kanban),
                []);

            var thens = new[]
            {
            new Then(
                "Test Then",
                TargetSystem.Kanban,
                Guid.NewGuid(),
                new Dictionary<string, string>())
            };

            var automation = new Automation(
                automationId,
                "Test Automation",
                true,
                true,
                when,
                thens);

            var actor = new Actor(
                "Test Actor",
                "Test Actor Type");

            var payload = System.Text.Json.JsonDocument
                .Parse("{}")
                .RootElement
                .Clone();

            var integrationEvent = new IntegrationEvent(
                Guid.NewGuid(),
                "Test Event",
                SourceSystem.Kanban,
                0,
                correlationId,
                Guid.NewGuid(),
                actor,
                payload);

            return new EvaluationContext(automation, integrationEvent);
        }
    }
}
