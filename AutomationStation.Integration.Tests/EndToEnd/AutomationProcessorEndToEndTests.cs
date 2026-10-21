using Dapper;

using Microsoft.Extensions.Logging.Abstractions;

using System.Data.Common;
using System.Text.Json;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Events;
using AutomationStation.Application.Models;
using AutomationStation.Application.Policies;
using AutomationStation.Application.Services;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Infrastructure.Actions;
using AutomationStation.Infrastructure.Limiting;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;

namespace AutomationStation.Integration.Tests.EndToEnd
{
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationProcessorEndToEndTests : IAsyncLifetime
    {
        private const string EventType = "PlacementCreated";
        private const string CreatePlacementAction = "CreatePlacement";
        private const string CreateTojTaskAction = "CreateTask";


        private readonly DatabaseFixture _database;
        private readonly RecordingActionHandler _handler = new(CreatePlacementAction, TargetSystem.Kanban);
        private readonly RecordingActionHandler _tojHandler = new(CreateTojTaskAction, TargetSystem.TojSystem);
        private readonly EventMessageHandler _messageHandler;

        public AutomationProcessorEndToEndTests(DatabaseFixture database)
        {
            _database = database;
            _messageHandler = CreateMessageHandler();
        }

        private EventMessageHandler CreateMessageHandler()
        {
            var connectionFactory = _database.ConnectionFactory;

            var processor = new AutomationProcessor(
                new AutomationRepository(connectionFactory),
                new ConditionEvaluator(new EventValueResolver()),
                new AutomationExecutionGuard(
                    [new CorrelationPolicy(new HistoryRepository(connectionFactory))],
                    NullLogger<AutomationExecutionGuard>.Instance),
                new ActionDispatcher(
                    [_handler, _tojHandler],
                    new InMemoryActionRateLimiter(TimeProvider.System),
                    new ActionCatalog(),
                    new AutomationExecutorRepository(connectionFactory)),
                new AutomationExecutionRepository(connectionFactory),
                new AutomationExecutionLimiter(connectionFactory),
                NullLogger<AutomationProcessor>.Instance);

            return new EventMessageHandler(
                new ProcessedMessageRepository(connectionFactory),
                processor,
                NullLogger<EventMessageHandler>.Instance);
        }

        public async ValueTask InitializeAsync() => await _database.ResetAsync();
        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task ProcessAsync_MatchingEvent_DispatchesActionAndRecordsExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, CreatePlacementAction, executionOrder: 1, cancellationToken);

            // Act
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            var outcome = await _messageHandler.HandleAsync(integrationEvent, cancellationToken);

            // Assert
            Assert.Equal(MessageOutcome.Processed, outcome);
            Assert.Equal(1, _handler.ExecutionCount);

            var executions = await connection.QueryAsync(
                new CommandDefinition(
                    """
                    SELECT AutomationId, EventId
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                      AND EventId = @EventId
                    """,
                    new
                    {
                        AutomationId = automationId,
                        integrationEvent.EventId
                    },
                    cancellationToken: cancellationToken));

            Assert.Single(executions);
        }

        [Fact]
        public async Task HandleAsync_WhenEventAlreadyProcessed_ReturnsDuplicateAndDoesNotDispatchAgain()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, CreatePlacementAction, executionOrder: 1, cancellationToken);

            // Act
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            await _messageHandler.HandleAsync(integrationEvent, cancellationToken);
            var outcome = await _messageHandler.HandleAsync(integrationEvent, cancellationToken);

            // Assert
            Assert.Equal(MessageOutcome.Duplicate, outcome);
            Assert.Equal(1, _handler.ExecutionCount);

            var executions = await connection.QueryAsync(
                new CommandDefinition(
                    """
                    SELECT AutomationId, EventId
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                      AND EventId = @EventId
                    """,
                    new
                    {
                        AutomationId = automationId,
                        integrationEvent.EventId
                    },
                    cancellationToken: cancellationToken));

            Assert.Single(executions);
        }

        [Fact]
        public async Task HandleAsync_WithDisabledExecutor_ThrowsInvalidOperationException()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken, targetSystem: TargetSystem.TojSystem, enabled: false);
            await InsertAutomationActionAsync(
                connection,
                automationId,
                executorId,
                CreateTojTaskAction,
                executionOrder: 1,
                cancellationToken,
                targetSystem: TargetSystem.TojSystem);

            // Act
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _messageHandler.HandleAsync(integrationEvent, cancellationToken));

            Assert.Equal(0, _tojHandler.ExecutionCount);
        }

        [Fact]
        public async Task ProcessAsync_WhenEventTypeDoesNotMatch_DoesNotDispatchAction()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, CreatePlacementAction, executionOrder: 1, cancellationToken);

            // Act
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: "PlacementDeleted",
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            var outcome = await _messageHandler.HandleAsync(integrationEvent, cancellationToken);

            // Assert
            Assert.Equal(MessageOutcome.Processed, outcome);
            Assert.Equal(0, _handler.ExecutionCount);

            var executions = await connection.QueryAsync(
                new CommandDefinition(
                    """
                    SELECT AutomationId, EventId
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                      AND EventId = @EventId
                    """,
                    new
                    {
                        AutomationId = automationId,
                        integrationEvent.EventId
                    },
                    cancellationToken: cancellationToken));

            Assert.Empty(executions);
        }

        [Fact]
        public async Task ProcessAsync_WhenTriggerSourceDoesNotMatch_DoesNotDispatchAction()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, CreatePlacementAction, executionOrder: 1, cancellationToken);

            // Act
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.TojSystem,
                CompanyId: 1,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            var outcome = await _messageHandler.HandleAsync(integrationEvent, cancellationToken);

            // Assert
            Assert.Equal(MessageOutcome.Processed, outcome);
            Assert.Equal(0, _handler.ExecutionCount);

            var executions = await connection.QueryAsync(
                new CommandDefinition(
                    """
                    SELECT AutomationId, EventId
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                      AND EventId = @EventId
                    """,
                    new
                    {
                        AutomationId = automationId,
                        integrationEvent.EventId
                    },
                    cancellationToken: cancellationToken));

            Assert.Empty(executions);
        }

        [Fact]
        public async Task ProcessAsync_WhenSameCorrelationIdOccursAgain_DoesNotDispatchAction()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            await InsertAutomationAsync(connection, automationId, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, EventType, cancellationToken);
            await InsertAutomationExecutorAsync(connection, executorId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, CreatePlacementAction, executionOrder: 1, cancellationToken);

            // Act
            var integrationEvent1 = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: correlationId,
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            var integrationEvent2 = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: EventType,
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: correlationId,
                CausationEventId: null,
                Actor: new Actor("-1", "TestActor"),
                Payload: JsonSerializer.SerializeToElement(new { columnId = $"{Guid.NewGuid()}" }));

            await _messageHandler.HandleAsync(integrationEvent1, cancellationToken);
            var outcome = await _messageHandler.HandleAsync(integrationEvent2, cancellationToken);

            // Assert
            Assert.Equal(MessageOutcome.Processed, outcome);
            Assert.Equal(1, _handler.ExecutionCount);

            var executions = await connection.QueryAsync(
                new CommandDefinition(
                    """
                    SELECT AutomationId, EventId
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                    """,
                    new
                    {
                        AutomationId = automationId
                    },
                    cancellationToken: cancellationToken));

            Assert.Single(executions);
        }

        private sealed class RecordingActionHandler(
            string actionType,
            TargetSystem targetSystem)
            : IActionHandler
        {
            public string ActionType { get; } = actionType;
            public TargetSystem TargetSystem { get; } = targetSystem;
            public int ExecutionCount { get; private set; }

            public Task ExecuteAsync(
                Then then,
                ActionContext context,
                CancellationToken cancellationToken)
            {
                ExecutionCount++;
                return Task.CompletedTask;
            }
        }

        private static async Task InsertAutomationAsync(
            DbConnection connection,
            Guid automationId,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations
                        (Id, Name, Enabled, AutomationTriggerable)
                    VALUES
                        (@AutomationId, 'Test Automation', 1, 1);
                    """,
                    new
                    {
                        AutomationId = automationId
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationTriggerAsync(
            DbConnection connection,
            Guid automationId,
            string eventType,
            CancellationToken cancellationToken,
            SourceSystem sourceSystem = SourceSystem.Kanban,
            string? configurationJson = null)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationTriggers
                        (Id, AutomationId, EventType, SourceSystem, ConfigurationJson)
                    VALUES
                        (NEWID(), @AutomationId, @EventType, @SourceSystem, @ConfigurationJson);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        EventType = eventType,
                        SourceSystem = sourceSystem.ToString(),
                        ConfigurationJson = configurationJson
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationExecutorAsync(
            DbConnection connection,
            Guid executorId,
            CancellationToken cancellationToken,
            TargetSystem targetSystem = TargetSystem.Kanban,
            bool enabled = true)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@ExecutorId, 1, @TargetSystem, 'ExecutorTest', @Enabled)
                    """,
                    new
                    {
                        ExecutorId = executorId,
                        TargetSystem = targetSystem.ToString(),
                        @Enabled = enabled
                    },
                    cancellationToken: cancellationToken));
        }

#pragma warning disable S107
        private static async Task InsertAutomationActionAsync(
            DbConnection connection,
            Guid automationId,
            Guid executorId,
            string actionType,
            int executionOrder,
            CancellationToken cancellationToken,
            TargetSystem targetSystem = TargetSystem.Kanban,
            string? configurationJson = null)
#pragma warning restore S107
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationActions
                        (Id, AutomationId, AutomationExecutorId, ActionType, TargetSystem, ConfigurationJson, ExecutionOrder)
                    VALUES
                        (NEWID(), @AutomationId, @ExecutorId, @ActionType, @TargetSystem, @ConfigurationJson, @ExecutionOrder);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ExecutorId = executorId,
                        ActionType = actionType,
                        TargetSystem = targetSystem.ToString(),
                        ExecutionOrder = executionOrder,
                        ConfigurationJson = configurationJson
                    },
                    cancellationToken: cancellationToken));
        }
    }
}
