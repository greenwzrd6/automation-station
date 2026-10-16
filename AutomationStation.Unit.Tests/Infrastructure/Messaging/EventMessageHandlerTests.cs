using Moq;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AutomationStation.Application.Events;

namespace AutomationStation.Unit.Tests.Infrastructure.Messaging
{
    public sealed class EventMessageHandlerTests
    {
        private readonly Mock<IProcessedMessageRepository> _messageRepositoryMock = new();
        private readonly Mock<IAutomationProcessor> _processorMock = new();
        private readonly Mock<ILogger<EventMessageHandler>> _loggerMock = new();
        private readonly EventMessageHandler _handler;

        public EventMessageHandlerTests()
        {
            _handler = new EventMessageHandler(
                _messageRepositoryMock.Object,
                _processorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenProcessingUnprocessedEvent_ReturnsProcessed()
        {
            // Arrange
            var integrationEvent = CreateEvent();
            var eventId = integrationEvent.EventId;

            _messageRepositoryMock
                .Setup(x => x.HasProcessedAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.HandleAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            Assert.Equal(MessageOutcome.Processed, result);

            _processorMock.Verify(
                x => x.ProcessAsync(
                    integrationEvent,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _messageRepositoryMock.Verify(
                x => x.MarkProcessedAsync(
                    eventId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _messageRepositoryMock.Verify(
                x => x.HasProcessedAsync(
                    eventId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenEventAlreadyProcessed_ReturnsDuplicateWithoutProcessing()
        {
            // Arrange
            var integrationEvent = CreateEvent();
            var eventId = integrationEvent.EventId;

            _messageRepositoryMock
                .Setup(x => x.HasProcessedAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.HandleAsync(
                integrationEvent,
                CancellationToken.None);

            // Assert
            Assert.Equal(MessageOutcome.Duplicate, result);

            _processorMock.Verify(
                x => x.ProcessAsync(
                    It.IsAny<IntegrationEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _messageRepositoryMock.Verify(
                x => x.MarkProcessedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenProcessorThrows_ThrowsInvalidOperationException()
        {
            // Arrange
            var integrationEvent = CreateEvent();
            var eventId = integrationEvent.EventId;

            _messageRepositoryMock
                .Setup(x => x.HasProcessedAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _processorMock
                .Setup(x => x.ProcessAsync(
                    It.IsAny<IntegrationEvent>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("bombaclat"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.HandleAsync(
                    integrationEvent,
                    CancellationToken.None));

            _messageRepositoryMock.Verify(
                x => x.MarkProcessedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static IntegrationEvent CreateEvent()
        {
            var payload = JsonDocument
                .Parse("{}")
                .RootElement
                .Clone();

            return new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: "Test Event",
                Source: SourceSystem.Kanban,
                CompanyId: 0,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("Test Actor", "Test Actor Type"),
                Payload: payload);
        }
    }
}
