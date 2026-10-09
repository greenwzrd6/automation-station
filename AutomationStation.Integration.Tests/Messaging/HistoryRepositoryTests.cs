using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;
using System.Text.Json;

namespace AutomationStation.Integration.Tests.Messaging
{
    /// <summary>
    /// Class <c>HistoryRepositoryTests</c> is the test class for <c>HistoryRepository</c>
    /// </summary>
    /// <param name="database">Primary constructor for <c>DatabaseFixture</c></param>
    [Collection(DatabaseCollection.Name)]
    public sealed class HistoryRepositoryTests(DatabaseFixture database)
    {
        private readonly DatabaseFixture _database = database;
        private readonly HistoryRepository _repository = new(
            database.ConnectionFactory);

        [Fact]
        public async Task CreateAutomationTimestampAsync_CreatesCorrectAutomationId_WhenUsed()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Act
            try
            {
                // Fake automation
                await InsertAutomationAsync(connection, automationId, cancellationToken);

                // Insert into history
                await _repository.CreateAutomationTimestampAsync(
                    context,
                    cancellationToken);

                // Assert
                var returnedAutomationId = await connection.QuerySingleAsync<Guid>(
                    new CommandDefinition(
                        """
                        SELECT AutomationId 
                        FROM AutomationHistory
                        WHERE AutomationId = @AutomationId;
                        """,
                        new { AutomationId = automationId },
                        cancellationToken: cancellationToken));

                Assert.Equal(automationId, returnedAutomationId);

                var returnedCorrelationId = await connection.QuerySingleAsync<Guid>(
                    new CommandDefinition(
                        """
                        SELECT CorrelationId 
                        FROM AutomationHistory
                        WHERE CorrelationId = @CorrelationId;
                        """,
                        new { CorrelationId = correlationId },
                        cancellationToken: cancellationToken));

                Assert.Equal(correlationId, returnedCorrelationId);
            }
            finally
            {
                // Cleanup
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        DELETE FROM AutomationHistory
                        WHERE AutomationId = @AutomationId;

                        DELETE FROM Automations
                        WHERE Id = @AutomationId;
                        """,
                        new { AutomationId = automationId },
                        cancellationToken: cancellationToken));
            }
        }

        [Fact]
        public async Task EventAlreadyProcessedAsync_ReturnsCorrectValue_WhenUsed()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Act/Assert
            try
            {
                // Fake automation
                await InsertAutomationAsync(connection, automationId, cancellationToken);

                var resultBeforeInsert = await _repository.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId,
                    cancellationToken);

                Assert.False(resultBeforeInsert);

                // Insert into history
                await _repository.CreateAutomationTimestampAsync(
                    context,
                    cancellationToken);

                var resultAfterInsert = await _repository.EventAlreadyProcessedAsync(
                    automationId,
                    correlationId,
                    cancellationToken);

                Assert.True(resultAfterInsert);
            }
            finally
            {
                // Cleanup
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        DELETE FROM AutomationHistory
                        WHERE AutomationId = @AutomationId;

                        DELETE FROM Automations
                        WHERE Id = @AutomationId;
                        """,
                        new { AutomationId = automationId },
                        cancellationToken: cancellationToken));
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

        private static EvaluationContext CreateContext(Guid automationId, Guid correlationId)
        {
            var automation = new Automation(
                id: automationId,
                name: "Test Automation",
                isEnabled: true,
                automationTriggerable: true,
                when: new When(
                    EventType: "PlacementCreated",
                    EventSource: SourceSystem.Kanban,
                    Conditions: []),
                thens: []);

            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: "PlacementCreated",
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: correlationId,
                CausationEventId: null,
                Actor: new Actor("-1", "User"),
                Payload: JsonSerializer.SerializeToElement(new { }));

            return new EvaluationContext(
                automation,
                integrationEvent);
        }
    }
}
