using Moq;

using Microsoft.Extensions.Logging;

using System.Text.Json;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Services;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Core.Automations.Whens;

namespace AutomationStation.Unit.Tests.Application.Services
{
    public sealed class AutomationProcessorTests
    {
        private readonly Mock<IAutomationRepository> _automationRepositoryMock = new();
        private readonly Mock<IConditionEvaluator> _conditionEvaluatorMock = new();
        private readonly Mock<IAutomationExecutionGuard> _executionGuardMock = new();
        private readonly Mock<IActionDispatcher> _actionDispatcherMock = new();
        private readonly Mock<IAutomationExecutionRepository> _executionRepositoryMock = new();
        private readonly Mock<IAutomationExecutionLimiter> _limiterMock = new();
        private readonly Mock<ILogger<AutomationProcessor>> _loggerMock = new();
        private readonly AutomationProcessor _processor;

        public AutomationProcessorTests()
        {
            _processor = new AutomationProcessor(
                _automationRepositoryMock.Object,
                _conditionEvaluatorMock.Object,
                _executionGuardMock.Object,
                _actionDispatcherMock.Object,
                _executionRepositoryMock.Object,
                _limiterMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessAsync_WhenNoAutomationsFound_DoesNothing()
        {
            // Arrange
            var integrationEvent = CreateEvent("PlacementCreated");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _executionGuardMock.Verify(
                x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _executionRepositoryMock.Verify(
                x => x.GetOrCreateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenEventTypeDiffers_SkipsAutomation()
        {
            // Arrange
            var automation = CreateAutomation(eventType: "PlacementCreated");
            var integrationEvent = CreateEvent("PlacementDeleted");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementDeleted",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _executionGuardMock.Verify(
                x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenSourceSystemDiffers_SkipsAutomation()
        {
            // Arrange
            var automation = CreateAutomation(source: SourceSystem.TojSystem);
            var integrationEvent = CreateEvent("PlacementCreated");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _executionGuardMock.Verify(
                x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenConditionDoesNotMatch_SkipsAutomation()
        {
            // Arrange
            var condition = new Condition("eventtype", ConditionOperator.Equals, "PlacementCreated");
            var automation = CreateAutomation(conditions: [condition]);
            var integrationEvent = CreateEvent("PlacementCreated");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            _conditionEvaluatorMock
                .Setup(x => x.Matches(
                    condition,
                    It.IsAny<IntegrationEvent>()))
                .Returns(false);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _executionGuardMock.Verify(
                x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenGuardBlocks_SkipsAutomation()
        {
            // Arrange
            var automation = CreateAutomation();
            var integrationEvent = CreateEvent("PlacementCreated");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            _executionGuardMock
                .Setup(x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _limiterMock.Verify(
                x => x.TryRecordExecutionAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenLimiterRejects_SkipsAutomation()
        {
            // Arrange
            var automation = CreateAutomation();
            var integrationEvent = CreateEvent("PlacementCreated");

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            _limiterMock
                .Setup(x => x.TryRecordExecutionAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _executionRepositoryMock.Verify(
                x => x.GetOrCreateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenEverythingPasses_DispatchesEachThen()
        {
            // Arrange
            ActionContext? capturedContext = null;
            var then1 = CreateThen("CreatePlacement");
            var then2 = CreateThen("UpdateTojTask");
            var automation = CreateAutomation(thens: [then1, then2]);
            var integrationEvent = CreateEvent("PlacementCreated");

            var executionId = Guid.NewGuid();

            _automationRepositoryMock
                .Setup(x => x.GetEnabledByEventTypeAsync(
                    "PlacementCreated",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([automation]);

            _executionGuardMock
                .Setup(x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _limiterMock
                .Setup(x => x.TryRecordExecutionAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _executionRepositoryMock
                .Setup(x => x.GetOrCreateAsync(
                    automation.Id,
                    integrationEvent.EventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(executionId);

            _actionDispatcherMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Then, ActionContext, CancellationToken>(
                    (_, context, _) => capturedContext = context);

            // Act
            await _processor.ProcessAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            _executionRepositoryMock.Verify(
                x => x.GetOrCreateAsync(
                    automation.Id,
                    integrationEvent.EventId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _actionDispatcherMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Then>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            Assert.NotNull(capturedContext);
            Assert.Equal(executionId, capturedContext.ExecutionId);

            _executionGuardMock.Verify(
                x => x.IsBlockedAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _limiterMock.Verify(
                x => x.TryRecordExecutionAsync(
                    It.IsAny<EvaluationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static Then CreateThen(string type)
        {
            return new Then(
                type,
                TargetSystem.Kanban,
                Guid.NewGuid(),
                new Dictionary<string, string>());
        }

        private static Automation CreateAutomation(
            Guid? id = null,
            string eventType = "PlacementCreated",
            SourceSystem source = SourceSystem.Kanban,
            IReadOnlyList<Condition>? conditions = null,
            IReadOnlyList<Then>? thens = null)
        {
            var when = new When(
                new Trigger(eventType, source),
                conditions ?? []);

            return new Automation(
                id ?? Guid.NewGuid(),
                "Test Automation",
                isEnabled: true,
                automationTriggerable: true,
                when,
                thens ?? []);
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
