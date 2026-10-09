using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;

namespace AutomationStation.Integration.Tests.Messaging
{
    [Collection(DatabaseCollection.Name)]
    public sealed class HistoryRepositoryTests(DatabaseFixture database)
    {
        private readonly DatabaseFixture _database = database;
        private readonly HistoryRepository _repository = new(
            database.ConnectionFactory);

        [Fact]
        public async Task Creates_AutomationTimestamp_WhenUsingCreateAutomationTimestampAsync()
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
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO Automations
                            (Id, Name, Enabled, AutomationTriggerable )
                        VALUES
                            (@AutomationId, 'Test Automation', 1, 1);
                        """,
                        new { AutomationId = automationId },
                        cancellationToken: cancellationToken));

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
