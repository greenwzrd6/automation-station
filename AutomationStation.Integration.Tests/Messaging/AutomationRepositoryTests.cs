using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;

namespace AutomationStation.Integration.Tests.Messaging
{
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationRepositoryTests(DatabaseFixture database)
    {
        private readonly DatabaseFixture _database = database;
        private readonly AutomationRepository _repository = new(
            database.ConnectionFactory);

        [Fact]
        public async Task Returns_AutomationsEnabledByEvent_WhenUsingGetEnabledByEventTypeAsync()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var executorId = Guid.NewGuid();
            var eventType = "TestEventType";

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            try
            {
                // Fake insertions
                await InsertAutomationAsync(connection, automationId, cancellationToken);
                await InsertAutomationTriggerAsync(connection, automationId, eventType, cancellationToken);
                await InsertAutomationConditionAsync(connection, automationId, cancellationToken);
                await InsertAutomationActionAsync(connection, automationId, executorId, cancellationToken);

                // Act
                var automations = await _repository.GetEnabledByEventTypeAsync(eventType, cancellationToken);

                var automation = Assert.Single(
                    automations,
                    automation => automation.Id == automationId);

                // Assert
                Assert.Equal(eventType, automation.When.EventType);
                Assert.True(automation.IsEnabled);
            }
            finally
            {
                // Cleanup
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        DELETE FROM AutomationActions
                        WHERE AutomationId = @AutomationId;

                        DELETE FROM AutomationExecutors
                        WHERE Id = @ExecutorId;

                        DELETE FROM AutomationConditions
                        WHERE AutomationId = @AutomationId;

                        DELETE FROM AutomationTriggers
                        WHERE AutomationId = @AutomationId;

                        DELETE FROM Automations
                        WHERE Id = @AutomationId;
                        """,
                        new
                        {
                            AutomationId = automationId,
                            ExecutorId = executorId
                        },
                        cancellationToken: CancellationToken.None));
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
                    new { AutomationId = automationId },
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
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationConditions
                        (Id, AutomationId, ConditionType, SourceSystem, ConfigurationJson)
                    VALUES
                        (NEWID(), @AutomationId, 'TestType', NULL, @ConfigurationJson);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ConfigurationJson = """
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
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@ExecutorId, 1, 'TestSystem', 'Test System', 1);
                    """,
                    new { ExecutorId = executorId },
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationActions
                        (Id, AutomationId, AutomationExecutorId, ActionType, TargetSystem, ConfigurationJson, ExecutionOrder)
                    VALUES
                        (NEWID(), @AutomationId, @ExecutorId, 'TestType', 'TestSystem', @ConfigurationJson, DEFAULT);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ExecutorId = executorId,
                        ConfigurationJson = """
                            {
                            "columnId": "test-column"
                            }
                            """
                    },
                    cancellationToken: cancellationToken));
        }
    }
}
