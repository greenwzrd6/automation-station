using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;

namespace AutomationStation.Integration.Tests.Messaging
{

    /// <summary>
    /// Class <c>AutomationRepositoryTests</c> is the test class for <c>AutomationRepository</c>
    /// </summary>
    /// <param name="database">Primary constructor for <c>DatabaseFixture</c></param>
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationRepositoryTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;
        private readonly AutomationRepository _repository = new(
            database.ConnectionFactory);

        public async ValueTask InitializeAsync()
        {
            await _database.ResetAsync();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenUsed_ShouldReturnAutomationsEnabledByEvent()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, actionType: "Yes", executionOrder: 0, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

            // Assert
            var automation = Assert.Single(
                automations,
                automation => automation.Id == automationId);

            Assert.Equal(eventType, automation.When.EventType);
            Assert.True(automation.IsEnabled);

            var condition = Assert.Single(automation.When.Conditions);

            Assert.Equal("columnId", condition.Field);
            Assert.Equal(ConditionOperator.Equals, condition.Operator);
            Assert.Equal("test-column", condition.Value);

            var action = Assert.Single(automation.Thens);

            Assert.Equal("Yes", action.Type);
            Assert.Equal(TargetSystem.Kanban, action.TargetSystem);
            Assert.Equal(executorId, action.ExecutorId);

            Assert.Single(action.Parameters);
            Assert.Equal("test-column", action.Parameters["columnId"]);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenGettingByNonExistantEventType_ShouldNotReturnAutomations()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, actionType: "Yes", executionOrder: 0, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync("TestEsnentRype", cancellationToken);

            // Assert
            Assert.DoesNotContain(automations, automation => automation.Id == automationId);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenNoDataInserted_ShouldNotReturnAnyAutomations()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync("hihihihi", cancellationToken);

            //Assert
            Assert.Empty(automations);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenInsertingManyAutomations_ShouldReturnAllAutomationsEnabledByEvent()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId1 = Guid.NewGuid();
            var automationId2 = Guid.NewGuid();
            var automationId3 = Guid.NewGuid();
            var executorId1 = Guid.NewGuid();
            var executorId2 = Guid.NewGuid();
            var executorId3 = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId1, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId1, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId1, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId1, executorId1, actionType: "Yes", executionOrder: 0, cancellationToken);

            await InsertAutomationAsync(connection, automationId2, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId2, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId2, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId2, executorId2, actionType: "Yes", executionOrder: 0, cancellationToken);

            await InsertAutomationAsync(connection, automationId3, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId3, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId3, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId3, executorId3, actionType: "Yes", executionOrder: 0, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

            // Assert
            Assert.Equal(3, automations.Count);

            var automation1 = Assert.Single(
                automations,
                automation => automation.Id == automationId1);
            Assert.Equal(eventType, automation1.When.EventType);
            Assert.True(automation1.IsEnabled);

            var automation2 = Assert.Single(
                automations,
                automation => automation.Id == automationId2);
            Assert.Equal(eventType, automation2.When.EventType);
            Assert.True(automation2.IsEnabled);

            var automation3 = Assert.Single(
                automations,
                automation => automation.Id == automationId3);
            Assert.Equal(eventType, automation3.When.EventType);
            Assert.True(automation3.IsEnabled);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenInsertingManyActionsWithExecutionOrder_ShouldReturnAllAutomationActionsInOrder()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId1 = Guid.NewGuid();
            var executorId2 = Guid.NewGuid();
            var executorId3 = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId1, actionType: "ThirdAction", executionOrder: 3, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId2, actionType: "FirstAction", executionOrder: 1, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId3, actionType: "SecondAction", executionOrder: 2, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

            // Assert
            var automation = Assert.Single(
                automations,
                automation => automation.Id == automationId);
            Assert.Equal(
                ["FirstAction", "SecondAction", "ThirdAction"],
                automation.Thens.Select(action => action.Type));
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenNotInsertingActionsOrCondition_ActionsAndConditionShouldBeEmpty()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

            // Assert
            var automation = Assert.Single(
                automations,
                automation => automation.Id == automationId);
            Assert.Empty(automation.When.Conditions);
            Assert.Empty(automation.Thens);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenNoConditionConfigurationJson_ShouldThrowError()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken, configurationJson: "");
            await InsertAutomationActionAsync(connection, automationId, executorId, actionType: "Yes", executionOrder: 0, cancellationToken);

            // Act and Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken));

            Assert.Equal("Condition configuration is missing.", exception.Message);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenNoActionTargetSystem_ShouldThrowError()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: true, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, actionType: "Yes", executionOrder: 0, cancellationToken, targetSystem: null);

            // Act and Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken));

            Assert.Equal($"Action 'Yes' in automation '{automationId}' has no TargetSystem.", exception.Message);
        }

        [Fact]
        public async Task GetEnabledByEventTypeAsync_WhenInsertingDisabledAutomation_ShouldNotReturnDisabledAutomation()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = $"TestEvent-{Guid.NewGuid()}";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake insertions
            await InsertAutomationAsync(connection, automationId, enabled: false, cancellationToken);
            await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
            await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
            await InsertAutomationActionAsync(connection, automationId, executorId, actionType: "Yes", executionOrder: 0, cancellationToken);

            // Act
            var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

            // Assert
            Assert.DoesNotContain(automations, automation => automation.Id == automationId);
        }

        private static async Task InsertAutomationAsync(
            DbConnection connection,
            Guid automationId,
            bool enabled,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations
                        (Id, Name, Enabled, AutomationTriggerable)
                    VALUES
                        (@AutomationId, 'Test Automation', @Enabled, 1);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        Enabled = enabled
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationTriggerAsync(
            DbConnection connection,
            Guid automationId,
            string eventType,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationTriggers
                        (Id, AutomationId, EventType, SourceSystem, ConfigurationJson)
                    VALUES
                        (NEWID(), @AutomationId, @EventType, NULL, NULL);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        EventType = eventType,
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationConditionAsync(
            DbConnection connection,
            Guid automationId,
            CancellationToken cancellationToken,
            string? configurationJson = null)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationConditions
                        (Id, AutomationId, ConditionType, SourceSystem, ConfigurationJson)
                    VALUES
                        (NEWID(), @AutomationId, 'Yes', NULL, @ConfigurationJson);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ConfigurationJson =
                            configurationJson ??
                            """
                            {
                                "field": "columnId",
                                "operator": "Equals",
                                "value": "test-column"
                            }
                            """
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationActionAsync(
            DbConnection connection,
            Guid automationId,
            Guid executorId,
            string actionType,
            int executionOrder,
            CancellationToken cancellationToken,
            string? targetSystem = "Kanban")
        {

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@ExecutorId, 1, @TargetSystem, 'Test Executor', 1);
                    """,
                    new
                    {
                        ExecutorId = executorId,
                        TargetSystem = "Kanban"
                    },
                    cancellationToken: cancellationToken));

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
                        TargetSystem = targetSystem,
                        ExecutionOrder = executionOrder,
                        ConfigurationJson =
                            """
                            {
                            "columnId": "test-column"
                            }
                            """
                    },
                    cancellationToken: cancellationToken));
        }
    }
}
