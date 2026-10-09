using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Infrastructure.RateLimiting;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Text.Json;

namespace AutomationStation.Integration.Tests.Messaging
{

    /// <summary>
    /// Class <c>CorrelationLoopTests</c> is the test class for <c>CorrelationLoopRepository/CorrelationPolicy</c>
    /// </summary>
    /// <param name="database">Primary constructor for <c>DatabaseFixture</c></param>
    [Collection(DatabaseCollection.Name)]
    public sealed class CorrelationLoopTests(DatabaseFixture database)
    {
        // Initializes the DatabaseFixture and repository
        private readonly DatabaseFixture _database = database;
        private readonly AutomationExecutionLimiter _repository = new(
                database.ConnectionFactory,
                new HistoryRepository(database.ConnectionFactory));

        [Fact]
        public async Task TryRecordExecutionAsync_ShouldBlockNextExecution_WhenActorReachesAutomationLimit()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId = Guid.NewGuid();
            var actorId = $"test-{Guid.NewGuid()}";

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Act
            try
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                    """
                    INSERT INTO Automations
                        (Id, Name, Enabled, AutomationTriggerable)
                    VALUES
                        (@AutomationId, 'Test Automation', 1, 1)
                    """,
                    new { AutomationId = automationId },
                    cancellationToken: cancellationToken));

                // Seed limit - 1 executions inside the time window.
                await connection.ExecuteAsync(
                    new CommandDefinition(
                    """
                    ;WITH Numbers AS (
                        SELECT 1 AS Number
                        UNION ALL
                        SELECT Number + 1
                        FROM Numbers
                        WHERE Number < @Count
                    )
                    INSERT INTO AutomationHistory
                        (Id, AutomationId, ActorId, CorrelationId, CausationEventId, TriggeredAt)
                    SELECT
                        NEWID(),
                        @AutomationId,
                        @ActorId,
                        NEWID(),
                        NULL,
                        SYSUTCDATETIME()
                    FROM Numbers
                    OPTION (MAXRECURSION 0);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ActorId = actorId,
                        Count = limit - 1
                    },
                    cancellationToken: cancellationToken));

                // Assert
                // Check if the next execution is blocked (should not be blocked yet)
                var recordedAtLimit = await _repository.TryRecordExecutionAsync(
                    CreateContext(automationId, actorId),
                    CancellationToken.None);

                Assert.True(recordedAtLimit);

                // Check if the next execution is blocked (should be blocked now)
                var recordedOverLimit = await _repository.TryRecordExecutionAsync(
                    CreateContext(automationId, actorId),
                    CancellationToken.None);

                Assert.False(recordedOverLimit);

                var historyCount = await connection.QuerySingleAsync<int>(
                    new CommandDefinition(
                    """
                    SELECT COUNT(*)
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                      AND ActorId = @ActorId;
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ActorId = actorId
                    },
                    cancellationToken: cancellationToken));

                Assert.Equal(limit, historyCount);
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
                    cancellationToken: CancellationToken.None));
            }
        }

        private static EvaluationContext CreateContext(Guid automationId, string actorId)
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
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: new Actor(actorId, "User"),
                Payload: JsonSerializer.SerializeToElement(new { }));

            return new EvaluationContext(
                automation,
                integrationEvent);
        }
    }
}
